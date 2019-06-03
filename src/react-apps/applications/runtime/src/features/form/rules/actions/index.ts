import { ActionCreatorsMapObject, bindActionCreators } from 'redux';
import { IDataModelFieldElement } from '../';
import { store } from '../../../../store';
import * as FetchRuleModel from './fetch';
import * as RuleActions from './rule';

export interface IFormRulesActions extends ActionCreatorsMapObject {
  checkIfRuleShouldRun: (
    lastUpdatedComponentId: string,
    lastUpdatedDataBinding: IDataModelFieldElement,
    lastUpdatedDataValue: string,
    repeatingContainerId?: string,
  ) => RuleActions.ICheckIfRuleShouldRun;
  fetchRuleModel: (url: string) => FetchRuleModel.IFetchRuleModel;
  fetchRuleModelFulfilled: (formData: any) => FetchRuleModel.IFetchRuleModelFulfilled;
  fetchRuleModelRejected: (error: Error) => FetchRuleModel.IFetchRuleModelRejected;
}

const actions: IFormRulesActions = {
  checkIfRuleShouldRun: RuleActions.checkIfRuleShouldRun,
  fetchRuleModel: FetchRuleModel.fetchRuleModelAction,
  fetchRuleModelFulfilled: FetchRuleModel.fetchRuleModelFulfilledAction,
  fetchRuleModelRejected: FetchRuleModel.fetchRuleModelRejectedAction,
};

const FormRulesActions: IFormRulesActions = bindActionCreators<any, IFormRulesActions>(actions, store.dispatch);

export default FormRulesActions;
