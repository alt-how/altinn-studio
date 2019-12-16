import { Action } from 'redux';
import * as ActionTypes from '../types';
import { ITextResource } from './../../../../../types/global';

export interface IFetchTextResourcesFulfilled extends Action {
  language: string;
  resources: ITextResource[];
}

export interface IFetchTextResourcesRejected extends Action {
  error: Error;
}

export function fetchTextResources(): Action {
  return {
    type: ActionTypes.FETCH_TEXT_RESOURCES,
  };
}

export function fetchFormResourceFulfilled(language: string, resources: ITextResource[]): IFetchTextResourcesFulfilled {
  return {
    type: ActionTypes.FETCH_TEXT_RESOURCES_FULFILLED,
    language,
    resources,
  };
}

export function fetchFormResourceRejected(error: Error): IFetchTextResourcesRejected {
  return {
    type: ActionTypes.FETCH_TEXT_RESOURCES_REJECTED,
    error,
  };
}
