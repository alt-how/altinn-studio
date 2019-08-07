const altinnWindow = window as any;
const { origin, org, service, reportee } = altinnWindow;

export const verifySubscriptionUrl = `${origin}/api/v1/${org}/${service}/verifySubscription?partyId=${reportee}`;

export const languageUrl = `${origin}/${org}/${service}/api/Language/GetLanguageAsJSON`;

export const profileApiUrl = `${origin}/${org}/${service}/api/v1/profile/user`;

export const applicationMetadataApiUrl = `${origin}/${org}/${service}/api/v1/applicationmetadata`;

export const textResourcesUrl = `${origin}/${org}/${service}/api/textresources`;
