export type FieldDataType = 'Text' | 'Number' | 'Boolean';

export interface FieldDraft {
  id: string;
  serverId: string | null;
  name: string;
  dataType: FieldDataType;
  isRequired: boolean;
}

export function createEmptyField(): FieldDraft {
  return {
    id: crypto.randomUUID(),
    serverId: null,
    name: '',
    dataType: 'Number',
    isRequired: false,
  };
}

export function dataTypeToEnum(dt: FieldDataType): number {
  switch (dt) {
    case 'Text':
      return 0;
    case 'Number':
      return 1;
    case 'Boolean':
      return 2;
  }
}

export function parseServerDataType(value: string): FieldDataType {
  switch (value) {
    case 'Text':
      return 'Text';
    case 'Number':
      return 'Number';
    case 'Boolean':
      return 'Boolean';
    default:
      return 'Number';
  }
}
