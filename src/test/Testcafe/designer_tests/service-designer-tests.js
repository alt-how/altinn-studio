import { Selector, t } from 'testcafe';
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
    t.ctx.deltMessage = "Du har delt dine endringer";
    t.ctx.syncMessage = "Endringene er validert";
    await common.login(testUser.userEmail, testUser.password, loginPage);
  })

test('Sync a service with master', async () => {
  await t
    .navigateTo(app.baseUrl + 'designer/AutoTest/testcafe05#/aboutservice')
    .click(designer.lageNavigationTab)
    .click(designer.dropDown)
    .pressKey("enter")
    .click(designer.omNavigationTab)
    .click(designer.lageNavigationTab)
    .expect(designer.delEndringer.exists).ok()
    .click(designer.delEndringer)
    .expect(designer.commitMessageBox.exists).ok()
    .click(designer.commitMessageBox)
    .typeText(designer.commitMessageBox, "Sync service automated test", { replace: true })
    .expect(designer.validerEndringer.exists).ok()
    .click(designer.validerEndringer)
    .pressKey("tab")
    .pressKey("enter")
});

test('About page items and editing', async () => {
  const randNumOne = Math.floor(100 + Math.random() *900);
  const randNumTwo = Math.floor(100 + Math.random() *900);
  const randId = Math.floor(100000 + Math.random() * 900000);
  await t
    .navigateTo(app.baseUrl + 'designer/AutoTest/testcafe#/uieditor')
    .click(designer.omNavigationTab)
    .expect(designer.omTjenesteNavn.focused).notOk() 
    .click(designer.omTjenesteNavn)
    .typeText(designer.omTjenesteNavn, 'testcafe' + '_' + randNumOne.toString())
    .click(designer.omEndreTjenesteNavn)
    .expect(designer.omTjenesteNavn.getAttribute("value")).notContains(randNumOne.toString(), "Endre must be clicked for field to be editable!")
    .pressKey('ctrl+a')
    .pressKey('backspace')
    .typeText(designer.omTjenesteNavn, 'testcafe' + '_' + randNumTwo.toString())
    .expect(designer.omTjenesteNavn.getAttribute("value")).eql("testcafe05" + "_" + randNumTwo.toString())
    .expect(designer.omLagringsNavn.getAttribute("value")).notContains(randNumTwo.toString())
    .pressKey('tab')
    .click(designer.omTjenesteId)
    .pressKey('ctrl+a')
    .pressKey('backspace')
    .typeText(designer.omTjenesteId, String(randId))
    .click(designer.omKommentarer)
    .pressKey('ctrl+a')
    .pressKey('backspace')
    .typeText(designer.omKommentarer, 'Lorem ipsum dolor sit amet.')
    .expect(designer.omKommentarer.textContent).contains("Lorem")
})

test.skip('Repeating groups', async () => {
  await t
    .Click();
});