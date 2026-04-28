import { AssetConditionStatus } from './asset-condition-status';

export interface SystemAssetDetail {
  id: string;
  assetName: string;
  assetType: string;
  serialNumber: string;
  location: string;
  conditionStatus: AssetConditionStatus;
  purchaseDate: string;
  lastMaintenance: string | null;
  notes: string;
  managedBy: string;
  managedByName: string;          // resolved staff display name
}
