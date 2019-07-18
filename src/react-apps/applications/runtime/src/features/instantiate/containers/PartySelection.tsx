import { createStyles, Grid, Typography, withStyles, WithStyles } from '@material-ui/core';
import AddIcon from '@material-ui/icons/Add';
import * as React from 'react';
import { useSelector } from 'react-redux';
import AltinnAppTheme from 'Shared/theme/altinnAppTheme';
import { IAltinnWindow, IRuntimeState } from 'src/types';
import Header from '../../../shared/components/altinnAppHeader';
import AltinnParty from '../../../shared/components/altinnParty';
import AltinnPartySearch from '../../../shared/components/altinnPartySearch';
import LanguageActions from '../../../shared/resources/language/languageActions';
import { IParty } from '../../../shared/resources/party';
import PartyActions from '../../../shared/resources/party/partyActions';
import ProfileActions from '../../../shared/resources/profile/profileActions';
import { changeBodyBackground } from '../../../utils/bodyStyling';

const styles = createStyles({
  partySelectionPage: {
    backgroundColor: AltinnAppTheme.altinnPalette.primary.white,
    width: '100%',
    height: '100%',
    maxWidth: '780px',
    display: 'flex',
    flexDirection: 'column',
    alignSelf: 'center',
    padding: 12,
  },
  partySelectionTitle: {
    fontSize: '3rem',
    padding: 12,
  },
  loadMoreButton: {
    padding: 5,
    width: '100%',
    backgroundColor: AltinnAppTheme.altinnPalette.primary.white,
    border: `2px dotted ${AltinnAppTheme.altinnPalette.primary.blue}`,
  },
  loadMoreButtonIcon: {
    marginLeft: '1.5rem',
  },
  loadMoreButtonText: {
    fontSize: '1.2rem',
    marginLeft: '1.2rem',
    fontWeight: 'bold',
  },
});

export interface IPartySelectionProps extends WithStyles<typeof styles> {

}

function PartySelection(props: IPartySelectionProps) {
  changeBodyBackground(AltinnAppTheme.altinnPalette.primary.white);

  const language = useSelector((state: IRuntimeState) => state.language.language);
  const profile = useSelector((state: IRuntimeState) => state.profile.profile);
  const parties = useSelector((state: IRuntimeState) => state.party.parties);

  const [filterString, setFilterString] = React.useState('');
  const [numberOfPartiesShown, setNumberOfPartiesShown] = React.useState(4);

  const { classes } = props;

  React.useEffect(() => {
    const {org, service} = window as IAltinnWindow;
    PartyActions.getParties(`${window.location.origin}/${org}/${service}/api/v1/parties`);
    if (!profile) {
      ProfileActions.fetchProfile(
        `${window.location.origin}/${org}/${service}/api/v1/profile/user`,
      );
    }
    if (!language) {
      LanguageActions.fetchLanguage(
        `${window.location.origin}/${org}/${service}/api/Language/GetLanguageAsJSON`,
        'nb',
      );
    }
  }, []);

  function renderParties() {
    if (!parties || !profile) {
      return null;
    }
    // Set the current selected party first in the array and concat rest (without the current)
    const currentParty: IParty = parties.find((party) => party.partyId === profile.partyId);
    const partiesElements = [currentParty].concat(
      parties.map(
        (party) => party.partyId !== currentParty.partyId ? party : null,
      ).filter(
          (party) => party != null,
        ),
    );
    return (
      <>
        {partiesElements.map((party: IParty, index: number) =>
          index < numberOfPartiesShown ?
            party.name.toUpperCase().includes(filterString.toUpperCase()) ?
              <AltinnParty
                key={index}
                party={party}
                isCurrent={party.partyId === currentParty.partyId}
              />
              : null
            : null,
          )}
      </>
    );
  }

  function onFilterStringChange(filterStr: string) {
    console.log(filterStr);
    setFilterString(filterStr);
  }

  function increaseNumberOfShownParties() {
    setNumberOfPartiesShown(numberOfPartiesShown + 4);
  }

  function renderShowMoreButton() {
    if (!parties || numberOfPartiesShown >= parties.length) {
      return null;
    }
    return (
      <button
        className={classes.loadMoreButton}
        onClick={increaseNumberOfShownParties}
      >
        <Grid container={true}>
            <AddIcon className={classes.loadMoreButtonIcon}/>
            <Typography className={classes.loadMoreButtonText}>
              Last flere
            </Typography>
        </Grid>
      </button>
    );
  }

  return (
    <Grid container={true} className={classes.partySelectionPage}>
      <Header
        language={language}
        profile={profile}
      />
        <Grid container={true}>
          <Typography className={classes.partySelectionTitle}>
            Hvem vil du sende inn for?
          </Typography>
        </Grid>
        <Grid container={true}>
          <AltinnPartySearch
            onSearchUpdated={onFilterStringChange}
          />
        </Grid>
        <Grid container={true}>
          <Typography variant='headline'>
            Dine aktører som kan starte tjenesten:
          </Typography>
          {renderParties()}
        </Grid>
        <Grid container={true}>
          {renderShowMoreButton()}
        </Grid>
    </Grid>
  );
}

export default withStyles(styles)(PartySelection);
