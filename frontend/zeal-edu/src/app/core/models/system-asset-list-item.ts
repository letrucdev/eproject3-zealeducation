import { AssetConditionStatus } from './asset-condition-status';

export interface SystemAssetListItem {
  id: string;
  assetName: string;
  assetType: string;
  serialNumber: string;
  location: string;
  conditionStatus: AssetConditionStatus;
  purchaseDate: string;           // ISO 8601
  lastMaintenance: string | null;
  notes: string;
  managedBy: string;              // staff UUID
  managedByName: string;          // resolved staff display name
}
