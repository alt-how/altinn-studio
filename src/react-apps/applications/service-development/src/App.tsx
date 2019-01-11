/* tslint:disable:jsx-no-lambda */
// https://github.com/facebook/create-react-app/issues/4801#issuecomment-409553780
// Disabled for React Router rendering

/* tslint:disable:jsx-boolean-value */
// Extensive used in Material-UI's Grid

import Grid from '@material-ui/core/Grid';
import Hidden from '@material-ui/core/Hidden';
import { createMuiTheme, createStyles, MuiThemeProvider, withStyles, WithStyles } from '@material-ui/core/styles';
import * as React from 'react';
import { connect } from 'react-redux';
import { HashRouter as Router, Redirect, Route, withRouter } from 'react-router-dom';
import LeftDrawerMenu from '../../shared/src/navigation/drawer/LeftDrawerMenu';
import AppBarComponent from '../../shared/src/navigation/main-header/appBar';
import altinnTheme from '../../shared/src/theme/altinnStudioTheme';
import NavigationActionDispatcher from './actions/navigationActions/navigationActionDispatcher';
import './App.css';
import { redirects } from './config/redirects';
import { routes } from './config/routes';
import fetchLanguageDispatcher from './utils/fetchLanguage/fetchLanguageDispatcher';

import HandleMergeConflict from './features/handleMergeConflict/HandleMergeConflictContainer';
import HandleMergeConflictDispatchers from './features/handleMergeConflict/handleMergeConflictDispatcher';
import { makeGetRepoStatusSelector } from './features/handleMergeConflict/handleMergeConflictSelectors';
import { compose } from 'redux';


// import * as networking from '../../../applications/shared/src/utils/networking';

const theme = createMuiTheme(altinnTheme);

const styles = () => createStyles({
  container: {
    backgroundColor: theme.altinnPalette.primary.greyLight,
    height: '100%',
    width: '100%',
  },
  subApp: {
    [theme.breakpoints.up('md')]: {
      paddingLeft: 73,
    },
    height: '100%',
    width: '100%',
  },
});

export interface IServiceDevelopmentProps extends WithStyles<typeof styles> {
  language: any;
  location: any;
  repoStatus: any;
}
export interface IServiceDevelopmentAppState {
  forceRepoStatusCheckComplete: boolean;

}

class App extends React.Component<IServiceDevelopmentProps, IServiceDevelopmentAppState> {
  constructor(_props: IServiceDevelopmentProps, _state: IServiceDevelopmentAppState) {
    super(_props, _state);
    this.state = {
      forceRepoStatusCheckComplete: true,
    };
  }

  public checkForMergeConflict = () => {
    const altinnWindow: any = window;
    const { org, service } = altinnWindow;
    // tslint:disable-next-line:max-line-length
    const repoStatusUrl = `${altinnWindow.location.origin}/designerapi/Repository/RepoStatus?owner=${org}&repository=${service}`;

    HandleMergeConflictDispatchers.fetchRepoStatus(repoStatusUrl, org, service);
  }

  public forceRepoStatusCheck = () => {
    this.setState(
      {
        forceRepoStatusCheckComplete: false,
      },
    );
  }

  public componentDidMount() {
    const altinnWindow: Window = window;
    fetchLanguageDispatcher.fetchLanguage(
      `${altinnWindow.location.origin}/designerapi/Language/GetLanguageAsJSON`, 'nb');

    this.checkForMergeConflict();

  }

  public handleDrawerToggle = () => {
    NavigationActionDispatcher.toggleDrawer();
  }

  public render() {
    const { classes, repoStatus } = this.props;
    const { forceRepoStatusCheckComplete } = this.state;
    const altinnWindow: IAltinnWindow = window as IAltinnWindow;
    const { org, service } = altinnWindow;

    return (
      <React.Fragment>
        <MuiThemeProvider theme={theme}>
          <Router>
            <div className={classes.container}>
              <Grid container={true} direction='row' id='test'>
                <Grid item={true} xs={12}>
                  {redirects.map((route, index) => (
                    <Route
                      key={index}
                      exact={true}
                      path={route.from}
                      render={() => (
                        <Redirect to={route.to} />
                      )}
                    />
                  ))}
                  {routes.map((route, index) => (
                    <Route
                      key={index}
                      path={route.path}
                      exact={route.exact}
                      render={(props) => <AppBarComponent
                        {...props}
                        org={org}
                        service={service}
                        showBreadcrumbOnTablet={true}
                        showSubHeader={true}
                        activeSubHeaderSelection={route.activeSubHeaderSelection}
                        activeLeftMenuSelection={route.activeLeftMenuSelection}
                      />}
                    />
                  ))}
                </Grid>
                <Grid item={true} xs={12}>
                  <Hidden smDown>
                    <div style={{ top: 50 }}>
                      {routes.map((route, index) => (
                        <Route
                          key={index}
                          path={route.path}
                          exact={route.exact}
                          render={(props) => <LeftDrawerMenu
                            {...props}
                            menuType={route.menu}
                            activeLeftMenuSelection={route.activeLeftMenuSelection}
                          />}
                        />
                      ))}
                    </div>
                  </Hidden>
                  {forceRepoStatusCheckComplete === true &&
                    repoStatus.hasMergeConflict === false ?
                    <div className={classes.subApp}>
                      {routes.map((route, index) => (
                        <Route
                          key={index}
                          path={route.path}
                          exact={route.exact}
                          render={(props) => <route.subapp
                            {...props}
                            name={route.path}
                          />}
                        />
                      ))}
                    </div>
                    :
                    null
                  }
                  {repoStatus.hasMergeConflict === true ?
                    <div className={classes.subApp}>
                      <HandleMergeConflict
                        checkForMergeConflict={this.checkForMergeConflict}
                      />
                    </div>
                    :
                    null
                  }
                </Grid>
              </Grid>
            </div>
          </Router>
        </MuiThemeProvider>
      </React.Fragment>
    );
  }
}

const makeMapStateToProps = () => {
  const GetRepoStatusSelector = makeGetRepoStatusSelector();
  const mapStateToProps = (
    state: IServiceDevelopmentState,
  ) => {
    return {
      repoStatus: GetRepoStatusSelector(state),
      language: state.language,
    };
  };
  return mapStateToProps;
};

export default compose(
  withRouter,
  withStyles(styles),
  connect(makeMapStateToProps),
)(App);
