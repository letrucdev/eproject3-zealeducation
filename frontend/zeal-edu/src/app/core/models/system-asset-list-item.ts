import { AssetConditionStatus } from './asset-condition-status';

export interface SystemAssetListItem {
  id: string;
  assetName: string;
  assetType: string;
  serialNumber: string;
  location: string;
  conditionStatus: AssetConditionStatus;
  purchaseDate: string;           // yyyy-MM-dd
  lastMaintenance: string | null; // yyyy-MM-dd
  notes: string;
  managedBy: string;              // staff UUID
  managedByName: string;          // resolved staff display name
}
