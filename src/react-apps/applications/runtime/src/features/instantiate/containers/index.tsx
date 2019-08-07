import { createStyles, withStyles, WithStyles } from '@material-ui/core/styles';
import * as React from 'react';
import ContentLoader from 'react-content-loader';
import { useSelector } from 'react-redux';
import { Redirect } from 'react-router-dom';
import AltinnAppTheme from 'Shared/theme/altinnAppTheme';
import { IAltinnWindow, IRuntimeState } from 'src/types';
import AltinnModal from '../../../../../shared/src/components/AltinnModal';
import AltinnAppHeader from '../../../shared/components/altinnAppHeader';
import { IParty } from '../../../shared/resources/party';
import PartyActions from '../../../shared/resources/party/partyActions';
import ProfileActions from '../../../shared/resources/profile/profileActions';
import { changeBodyBackground } from '../../../utils/bodyStyling';
import { post } from '../../../utils/networking';
import SubscriptionHookError from '../components/subscriptionHookError';
import { verifySubscriptionHook } from '../resources/verifySubscriptionHook';

const styles = () => createStyles({
  modal: {
    boxShadow: null,
    MozBoxShadow: null,
    WebkitBoxShadow: null,
  },
  body: {
    padding: 0,
  },
});

export interface IPartyValidation {
  valid: boolean;
  message: string;
  validParties: IParty[];
}

export interface IServiceInfoProps extends WithStyles<typeof styles> {
  // intentionally left empty
}

function ServiceInfo(props: IServiceInfoProps) {
  changeBodyBackground(AltinnAppTheme.altinnPalette.primary.blue);
  const { org, service } = window as IAltinnWindow;

  const [instanceId, setInstanceId] = React.useState(null);
  const [instantiationError, setInstantiationError] = React.useState(null);
  const [subscriptionHookValid, setSubscriptionHookValid] = React.useState(false);
  const [partyValidation, setPartyValidation] = React.useState(null);

  const language = useSelector((state: IRuntimeState) => state.language.language);
  const profile = useSelector((state: IRuntimeState) => state.profile.profile);
  const selectedParty = useSelector((state: IRuntimeState) => state.party.selectedParty);
  const textResources = useSelector((state: IRuntimeState) => state.textResources.resources);

  const createNewInstance = async () => {
    try {
      const formData: FormData = new FormData();
      formData.append('PartyId', selectedParty.partyId.toString());
      formData.append('Org', org);
      formData.append('Service', service);
      const url = `${window.location.origin}/${org}/${service}/Instance/InstantiateApp`;
      const response = await post(url, null, formData);

      if (response.data.instanceId) {
        setInstanceId(response.data.instanceId);
        (window as IAltinnWindow).instanceId = response.data.instanceId;
      } else {
        throw new Error('Server did not respond with new instance');
      }
    } catch (err) {
      throw new Error('Server did not respond with new instance');
    }
  };

  const validatatePartySelection = async () => {
    try {
      if (!selectedParty) {
        return;
      }
      const { data } = await post(
        `${window.location.origin}/${org}/${service}` +
        `/api/v1/parties/validateInstantiation?partyId=${selectedParty.partyId}`,
      );
      setPartyValidation(data);
    } catch (err) {
      console.error(err);
      throw new Error('Server did not respond with party validation');
    }
  };

  const validateSubscriptionHook = async () => {
    await verifySubscriptionHook().then((result: boolean) => {
      setSubscriptionHookValid(result);
    });
  };

  const renderModalAndLoader = (): JSX.Element => {
    const {classes} = props;
    return (
      <>
        <AltinnModal
          classes={classes}
          isOpen={true}
          onClose={null}
          hideBackdrop={true}
          hideCloseIcon={true}
          headerText={'Instansierer'}
        >
          <ContentLoader
            height={200}
          >
            <rect x='25' y='20' rx='0' ry='0' width='100' height='5' />
            <rect x='25' y='30' rx='0' ry='0' width='350' height='5' />
            <rect x='25' y='40' rx='0' ry='0' width='350' height='25' />

            <rect x='25' y='75' rx='0' ry='0' width='100' height='5' />
            <rect x='25' y='85' rx='0' ry='0' width='350' height='5' />
            <rect x='25' y='95' rx='0' ry='0' width='350' height='25' />

            <rect x='25' y='130' rx='0' ry='0' width='100' height='5' />
            <rect x='25' y='140' rx='0' ry='0' width='350' height='5' />
            <rect x='25' y='150' rx='0' ry='0' width='350' height='25' />
          </ContentLoader>
        </AltinnModal>
      </>
    );
  };

  React.useEffect(() => {
    if (!profile) {
      ProfileActions.fetchProfile(`${window.location.origin}/${org}/${service}/api/v1/profile/user`);
    }
    if (!selectedParty) {
      PartyActions.selectParty(profile.party);
    }
    if (!partyValidation) {
      validatatePartySelection();
    }

    validateSubscriptionHook();

    if (!instanceId && instantiationError === null && partyValidation !== null) {
      if (subscriptionHookValid === true) {
        createNewInstance();
      }
    }
  }, [profile, instanceId, partyValidation, selectedParty]);

  if (partyValidation !== null && !partyValidation.valid) {
    if (partyValidation.validParties.length === 0) {
      return (
        <Redirect
          to={{
            pathname: '/error',
            state: {
              message: partyValidation.message,
            },
          }}
        />
      );
    } else {
      return (
        <Redirect
          to={{
            pathname: '/partyselection',
            state: {
              validParties: partyValidation.validParties,
            },
          }}
        />
      );
    }
  }

  if (partyValidation !== null && !partyValidation.valid) {
    return (
      <Redirect to={'/partyselection'}/>
    );
  }
  if (instanceId) {
    return (
      <Redirect to={`/instance/${instanceId}`} />
    );
  } else {
    return (
      <>
      <AltinnAppHeader profile={profile} language={language}/>
      {subscriptionHookValid && renderModalAndLoader()}
      {!subscriptionHookValid && <SubscriptionHookError textResources={textResources}/>}
      </>
    );
  }
}

export default withStyles(styles)(ServiceInfo);
