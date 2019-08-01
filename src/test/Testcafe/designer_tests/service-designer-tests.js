import { Selector, t } from 'testcafe';
import axeCheck from 'axe-testcafe';
import App from '../app';
import { AutoTestUser } from '../TestData';
import DesignerPage from '../page-objects/designerPage';

let app = new App();
let designer = new DesignerPage();

fixture('GUI service designer tests')
  .page(app.baseUrl)
  .beforeEach(async t => {
    t.ctx.deltMessage = "Du har delt dine endringer";
    t.ctx.syncMessage = "Endringene er validert";
    await t
      .useRole(AutoTestUser)
      .resizeWindow(1280, 610)
  })

test('Drag and drop test', async () => {
  await t
    .navigateTo(app.baseUrl + 'designer/AutoTest/auto_test#/uieditor')
    .expect(designer.inputComponent).ok()
    .dragToElement(designer.inputComponent, designer.dragToArea)
    .dragToElement(designer.addressComponent, designer.dragToArea)
})

test('Add one of each component to the designer using keyboard', async () => {
  await t
    .navigateTo(app.baseUrl + 'designer/AutoTest/auto_test#/aboutservice')
    .click(designer.lageNavigationTab)
    .expect(designer.inputComponent.visible).ok()
    .click(designer.inputComponent)
    .pressKey('enter') //input button
    .pressKey('tab')
    .pressKey('enter') //datepicker
    .pressKey('tab')
    .pressKey('enter') //dropdown
    .pressKey('tab')
    .pressKey('enter') //checkboxes
    .pressKey('tab')
    .pressKey('enter') //radiobutton
    .pressKey('tab')
    .pressKey('enter') //text area
    .pressKey('tab')
    .pressKey('enter') //file upload
    .pressKey('tab')
    .pressKey('enter') //submit
})

test('Sync a service with master', async () => {
  await t
    .navigateTo(app.baseUrl + 'designer/AutoTest/auto_test#/aboutservice')
    .click(designer.lageNavigationTab)
    .click(designer.hentEndringer)
  await t.eval(() => location.reload(true));
  await t
    .dragToElement(designer.inputComponent, designer.dragToArea)
    .click(designer.omNavigationTab)
    .click(designer.lageNavigationTab)
    .expect(designer.delEndringer.exists).ok()
    .click(designer.delEndringer)
    .expect(designer.commitMessageBox.exists).ok()
    .click(designer.commitMessageBox)
    .typeText(designer.commitMessageBox, "Sync service automated test", { replace: true })
    .expect(designer.validerEndringer.exists).ok()
    .click(designer.validerEndringer)
    .click(designer.delEndringerBlueButton)
    .expect(designer.ingenEndringer.exists).ok()
});

test('About page items and editing', async () => {
  const randNumOne = Math.floor(100 + Math.random() * 900);
  const randNumTwo = Math.floor(100 + Math.random() * 900);
  const randId = Math.floor(100000 + Math.random() * 900000);
  await t
    .navigateTo(app.baseUrl + 'designer/AutoTest/auto_test#/uieditor')
    .click(designer.omNavigationTab)
    .expect(designer.omTjenesteNavn.focused).notOk()
    .click(designer.omTjenesteNavn)
    .typeText(designer.omTjenesteNavn, 'autotest' + '_' + randNumOne.toString())
    .click(designer.omEndreTjenesteNavn)
    .expect(designer.omTjenesteNavn.getAttribute("value")).notContains(randNumOne.toString(), "Endre must be clicked for field to be editable!")
    .pressKey('ctrl+a')
    .pressKey('backspace')
    .typeText(designer.omTjenesteNavn, 'autotest' + '_' + randNumTwo.toString())
    .expect(designer.omTjenesteNavn.getAttribute("value")).eql("autotest" + "_" + randNumTwo.toString())
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

test("Fill out Access control information on a service", async () => {
  await t
    .navigateTo(app.baseUrl + 'designer/AutoTest/auto_test#/uieditor')
    .click(designer.lageNavigationTab)
    .hover(designer.leftDrawerMenu)
    .click(designer.lageLeftMenuItems[4])
    .click(designer.virksomhet)
    //.expect(designer.virksomhet.checked).ok()
    .click(designer.hookCheckBox)
    .expect(designer.tjenestekode.exists).ok()
    .typeText(designer.tjenestekode, '1234')
    .expect(designer.tjenesteutgavekode.exists).ok()
    .typeText(designer.tjenesteutgavekode, '1')
    .click(designer.virksomhet)
    //.expect(designer.virksomhet.checked).notOk()
    .click(designer.hookCheckBox)
    .expect(designer.tjenestekode.exists).notOk()
    .expect(designer.tjenesteutgavekode.exists).notOk()
})

test('Automated accessibility test for designer page', async t => {
  axeCheck(t);
})

test.skip('Repeating groups', async () => {
  await t
    .Click();
});