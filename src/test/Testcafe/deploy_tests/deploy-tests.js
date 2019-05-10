import { Selector, t } from 'testcafe';
import axeCheck from 'axe-testcafe';
import App from '../app';
import DashBoard from '../page-objects/DashboardPage';
import LoginPage from '../page-objects/loginPage';
import CommonPage from '../page-objects/common';
import TestData from '../TestData';
import DesignerPage from '../page-objects/designerPage';

let app = new App();
let dash = new DashBoard();
let loginPage = new LoginPage();
let common = new CommonPage();
let designer = new DesignerPage();
const testUser = new TestData('AutoTest', 'automatictestaltinn@brreg.no', 'test123', 'basic');

fixture('GUI service designer tests')
  .page(app.baseUrl)
  .beforeEach(async t => {
    t.ctx.klarForDeploy = "Tjenesten er klar til å legges ut i testmiljø";
    t.ctx.deployFailure = "Tjenesten ble ikke lagt ut i testmiljøet";
    t.ctx.tilgjengelig = "";
    t.ctx.ikkeTilgjengelig = "Tjenesten din er ikke tilgjengelig i testmiljø";
    await common.login(testUser.userEmail, testUser.password, loginPage);
  })

test.skip('Happy path; deploy a service to a test environment', async() => {
   header = Selector(headerString).withText(containedText);
  await t
    .navigateTo(app.baseUrl + 'designer/tdd/deployment#/deploytotest')
    .click(designer.testeNavigationTab)
    .hover(designer.leftDrawerMenu)
    .click(designer.testeLeftMenuItems[1])
    .expect(designer.deployButton.exists).ok()
    .click(designer.deployButton)
  let header = await Selector(headerString).withText(containedText);
  await t.expect(header.exists).ok();
})