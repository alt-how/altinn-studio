import { createStyles, Grid, Paper, Typography, withStyles, WithStyles } from '@material-ui/core';
import * as React from 'react';
import altinnTheme from '../../../../shared/src/theme/altinnAppTheme';
import { IParty } from '../resources/party';

const styles = createStyles({
  partyPaper: {
    'marginBottom': 12,
    'borderRadius': 0,
    'backgroundColor': altinnTheme.altinnPalette.primary.blueLight,
    'boxShadow': altinnTheme.sharedStyles.boxShadow,
    'width': '100%',
    '&:hover': {
      cursor: 'pointer',
    },
  },
  partyCurrent: {
    'marginBottom': 12,
    'borderRadius': 0,
    'backgroundColor': altinnTheme.altinnPalette.primary.greyLight,
    'boxShadow': altinnTheme.sharedStyles.boxShadow,
    'width': '100%',
    'cursor': 'point',
    '&:hover': {
      cursor: 'pointer',
    },
  },
  partyIcon: {
    fontSize: '36px',
    padding: 12,
    paddingTop: 18,
  },
  partyName: {
    paddingTop: 12,
    fontSize: '2rem',
    fontWeight: 700,
  },
  partyInfo: {
    fontSize: '1.5rem',
  },
});

export interface IAltinnPartyProps extends WithStyles<typeof styles> {
  party: IParty;
  isCurrent: boolean;
  onSelectParty: (party: IParty) => void;
}

function AltinnParty(props: IAltinnPartyProps) {
  const { classes, party, isCurrent, onSelectParty } = props;
  const isOrg: boolean = party.orgNumber != null;

  function onClickParty(event: React.MouseEvent<HTMLDivElement, MouseEvent>) {
    onSelectParty(party);
  }

  return (
    <Paper className={isCurrent ? classes.partyCurrent : classes.partyPaper} onClick={onClickParty}>
      <Grid container={true}>
        <Grid item={true}>
          <i className={classes.partyIcon + (isOrg ? ' fa fa-corp' : ' fa fa-private')}/>
        </Grid>
        <Grid item={true}>
          <Grid item={true}>
            <Typography className={classes.partyName}>
              {party.name + (party.isDeleted ? ' (slettet)' : '')}
            </Typography>
            <Typography className={classes.partyInfo}>
              {
                isOrg ?
                'Org.nr. ' + party.orgNumber :
                'Personnr. ' + party.ssn
              }
            </Typography>
          </Grid>
        </Grid>
      </Grid>
    </Paper>
  );
}

export default withStyles(styles)(AltinnParty);
