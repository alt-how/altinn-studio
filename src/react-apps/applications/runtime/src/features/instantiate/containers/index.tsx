import Grid from '@material-ui/core/Grid';
import { createStyles, withStyles } from '@material-ui/core/styles';
import * as React from 'react';
import ContentLoader from 'react-content-loader';
import { Redirect } from 'react-router-dom';
import { IAltinnWindow } from 'src/types';
import AltinnModal from '../../../../../shared/src/components/AltinnModal';
import { get, post } from '../../../utils/networking';
import Header from '../../header/containers';

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

function ServiceInfo(props) {
  const [reportee, setReportee] = React.useState(null);
  const [instanceId, setInstanceId] = React.useState(null);
  const [instantiationError, setInstantiationError] = React.useState(null);

  const fetchReportee = async () => {
    let routePrefix: string = null;
    if (window.location.origin.includes('altinn.studio') || window.location.origin.includes('altinn3.no')) {
      routePrefix = '/runtime';
    }
    const url: string = `${window.location.origin}${routePrefix}/api/v1/profile/user`;
    const fetchedReportee: any = await get(url);
    setReportee(fetchedReportee);
  };

  const createNewInstance = async () => {
    try {
      const { org, service } = window as IAltinnWindow;
      let routePrefix: string = null;
      if (window.location.origin.includes('altinn.studio') || window.location.origin.includes('altinn3.no')) {
        routePrefix = '/runtime';
      }
      const url = `${window.location.origin}${routePrefix}/Instance/InstantiateApp`;
      const formData: FormData = new FormData();
      formData.append('PartyId', reportee.userId);
      formData.append('Org', org);
      formData.append('Service', service);
      const response = await post(url, null, formData);

      if (response.data.instanceId) {
        setInstanceId(response.data.instanceId);
      } else {
        setInstantiationError(new Error(
          'Server responded without instanceId',
        ));
      }
    } catch (err) {
      alert(err);
    }
  };

  React.useEffect(() => {
    if (!reportee) {
      fetchReportee();
    }
    if (!instanceId && reportee !== null && instantiationError === null) {
      createNewInstance();
    }
  }, [reportee, instanceId]);

  if (instanceId) {
    return (
      <Redirect to={`/instance/${instanceId}`} />
    );
  } else {
    const { classes } = props;
    return (
      <>
      <Header />
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
  }
}

export default withStyles(styles)(ServiceInfo);
