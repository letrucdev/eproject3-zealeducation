import { AssetConditionStatus } from '../../../../core/models/asset-condition-status';

export interface CreateAssetPayload {
  assetName: string;
  assetType: string;
  serialNumber: string;
  location: string;
  purchaseDate: string;   // yyyy-MM-dd
  notes: string;
}

export interface UpdateAssetPayload {
  assetName: string;
  assetType: string;
  location: string;
  notes: string;
}

export interface UpdateAssetConditionPayload {
  conditionStatus: AssetConditionStatus;
}

export interface AssetListQuery {
  search?: string;
  assetType?: string;
  conditionStatus?: AssetConditionStatus;
}
