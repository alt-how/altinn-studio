import { Selector, t, ClientFunction } from 'testcafe';

export default class RunTimePage {
  constructor() {
    this.openManualTestWindow = Selector('#manual-test-button');
    this.testBrukerIframe = Selector('#root > div > div > div:nth-child(2) > div > div > iframe');
    this.testUsers = [
      Selector('strong').withText('Ola'),
      Selector('strong').withText('Kari'),
      Selector('strong').withText('Anne'),
      Selector('strong').withText('Pål')
    ];
    this.languageSelection = Selector('#reporteeLanguageSelect');
    this.changeLanguageButton = Selector('.btn.btn-primary').withAttribute('value', 'Oppdater språk');
    this.prefillData = Selector('#PrefillList');
    this.startNewButton = Selector('.btn.btn-primary').withAttribute('value', 'Start ny');
    this.backToAltinnStudio = Selector('.btn.btn-primary').withAttribute('value', 'Tilbake til Altinn Studio');

    //SBL components
    this.fileDropComponent = Selector('input').withAttribute('type', 'file');//Selector('.file-upload').child(0); 
    this.fileListBox = Selector('[id*="-fileuploader-"]');
    this.fileDeleteButton = Selector('#attachment-delete-0');
    this.checkBox = Selector('');
    this.textboxComponent = Selector('textarea')
    this.addressComponent = Selector('input').withAttribute('type', 'text');

    //file component error message
    this.errorMessage = Selector('.field-validation-error.a-message.a-message-error');

    //read-only components
    //this.readOnlyInput = readOnlySelectors('Navn');
  }

  async readOnlySelectors(innerText) {
    let readOnlySelector = Selector('.a-form-group').withText(innerText).child('input');
    return (readOnlySelector)
  }
}