import { SagaIterator } from 'redux-saga';
import { call, select, takeEvery } from 'redux-saga/effects';
import { IAltinnWindow } from '../..';
import { getFileUploadComponentValidations } from '../../../../../components/base/FileUploadComponent';
import { IRuntimeState } from '../../../../../types';
import { get, post } from '../../../../../utils/networking';
import FormFileUploadDispatcher from '../../actions';
import * as deleteActions from '../../actions/delete';
import * as FileUploadActionsTypes from '../../actions/types';

export function* watchDeleteAttachmentSaga(): SagaIterator {
  yield takeEvery(FileUploadActionsTypes.DELETE_ATTACHMENT, deleteAttachmentSaga);
}

export function* deleteAttachmentSaga(
  { attachment, attachmentType, componentId }: deleteActions.IDeleteAttachmentAction): SagaIterator {
  const state: IRuntimeState = yield select();
  const language = state.language.language;
  try {
    const altinnWindow: IAltinnWindow = window as IAltinnWindow;
    const { org, service, instanceId, reportee } = altinnWindow;
    const servicePath = `${org}/${service}`;
    const getDeleteUrl = `${altinnWindow.location.origin}/runtime/api/${reportee}/` +
      `${servicePath}/GetAttachmentDeleteUrl/${instanceId}/${attachmentType}/${attachment.name}/${attachment.id}`;
    const deleteUrl = yield call(get, getDeleteUrl);
    const response = yield call(post, deleteUrl);
    if (response.status === 200) {
      yield call(FormFileUploadDispatcher.deleteAttachmentFulfilled, attachment.id, attachmentType, componentId);
    } else {
      const validationMessages = getFileUploadComponentValidations('delete', language);
      yield call(FormFileUploadDispatcher.deleteAttachmentRejected,
        attachment, attachmentType, componentId, validationMessages);

    }
  } catch (err) {
    const validationMessages = getFileUploadComponentValidations('delete', language);
    yield call(FormFileUploadDispatcher.deleteAttachmentRejected,
      attachment, attachmentType, componentId, validationMessages);
    console.error(err);
  }
}
