import * as moment from 'moment';
import { IInstance, IParty } from '../../../shared/src/types';
import { getLanguageFromKey } from '../../../shared/src/utils/language';
import { getInstanceId } from './instance';

export const getInstanceMetaDataObject = (instance: IInstance, party: IParty,  language: any, organisations: any) => {
  const obj = {} as any;

  if (!instance || !party || !language || !organisations) {
    return obj;
  }

  let dateSubmitted;
  if (instance.data) {
    const lastChanged = instance.data.filter((elem) => elem.elementType === 'default')[0].lastChangedDateTime;
    dateSubmitted = moment(lastChanged).format('DD.MM.YYYY / HH:mm');
  }
  obj[getLanguageFromKey('receipt_platform.date_sent', language)] = dateSubmitted;
  let sender: string = '';
  if (party && party.ssn) {
    sender = `${party.person.ssn}-${party.person.name}`;
  } else if (party && party.orgNumber) {
    sender = `${party.orgNumber}-${party.organisation.name}`;
  }
  obj[getLanguageFromKey('receipt_platform.sender', language)] = sender;
  obj[getLanguageFromKey('receipt_platform.receiver', language)] = getOrganisationDisplayName(instance, organisations);
  obj[getLanguageFromKey('receipt_platform.reference_number', language)] = getInstanceId();
  return obj;
};

export const getOrganisationDisplayName = (instance: IInstance, organisations: any ): string => {
  if (!organisations) {
    return instance.org.toUpperCase();
  } else {
    // TODO: fetch this language based on language cookie
    if (organisations.orgs[instance.org]) {
        return organisations.orgs[instance.org].name.nb.toUpperCase();
    } else {
      return instance.org.toUpperCase();
    }
  }
};
