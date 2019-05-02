import { Selector, t, ClientFunction } from 'testcafe';

export default class RunTimePage {
  constructor() {
    this.openManualTestWindow = Selector('#manual-test-button');
    this.testUsers = [
      Selector('div').withText('Ola'),
      Selector('div').withText('Kari'),
      Selector('div').withText('Anne'),
      Selector('div').withText('Pål')
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
    this.textboxComponent = Selector('textarea')
    this.addressComponent = Selector('input').withAttribute('type','text');

    //read-only components
    //this.readOnlyInput = readOnlySelectors('Navn');
  }

  async readOnlySelectors(innerText){
    let readOnlySelector = Selector('.a-form-group').withText(innerText).child('input');
    return(readOnlySelector)
  }
}