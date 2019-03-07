import { combineReducers, Reducer } from 'redux';
import codeListsReducer, { ICodeListsState } from './codeListsReducer';
import dataModelReducer, { IDataModelState } from './dataModelReducer';
import languageReducer, { ILanguageState } from './languageReducer';
import ruleModelReducer, { IRuleModelState } from './ruleModelReducer';
import textResourceReducer, { ITextResourcesState } from './textResourcesReducer';
import thirdPartyComponentsReducer, { IThirdPartyComponentsState } from './thirdPartyComponentsReducer';

export interface IAppDataState {
  dataModel: IDataModelState;
  textResources: ITextResourcesState;
  ruleModel: IRuleModelState;
  codeLists: ICodeListsState;
  language: ILanguageState;
  thirdPartyComponents: IThirdPartyComponentsState;
}

const combinedReducers: Reducer<IAppDataState> = combineReducers({
  dataModel: dataModelReducer,
  textResources: textResourceReducer,
  ruleModel: ruleModelReducer,
  codeLists: codeListsReducer,
  language: languageReducer,
  thirdPartyComponents: thirdPartyComponentsReducer,
});

export default combinedReducers;
