import 'jest';
import { ITextResource } from '../../src/types/global';
import { getTextResourceByKey } from '../../src/utils/textResource';

describe('>>> /utils/textResource.ts', () => {
  let mockTextResources: ITextResource[];
  let mockKey: string;
  let mockInvalidKey: string;
  beforeEach(() => {
    mockTextResources = [{id: 'mockId1', value: 'mock value 1'}, {id: 'mockId2', value: 'mock value 2'} ];
    mockKey = 'mockId1';
    mockInvalidKey = 'invalid';
  });

  it('+++ should return correct value for a given key', () => {
    const result = getTextResourceByKey(mockKey, mockTextResources);
    expect(result).toBe(mockTextResources[0].value);
  });

  it('+++ should return the key if a value does not exist for the given key', () => {
    const result = getTextResourceByKey(mockInvalidKey, mockTextResources);
    expect(result).toBe(mockInvalidKey);
  });

  it('+++ should return key if mockTextResources are null', () => {
    const result = getTextResourceByKey(mockKey, null);
    expect(result).toBe(mockKey);
  });
});
