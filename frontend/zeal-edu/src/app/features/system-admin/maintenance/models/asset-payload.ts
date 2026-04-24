import { AssetConditionStatus } from '../../../../core/models/asset-condition-status';

export interface CreateAssetPayload {
  assetName: string;
  assetType: string;
  serialNumber: string;
  location: string;
  purchaseDate: string;   // ISO 8601
  notes: string;
}

export interface UpdateAssetPayload {
  assetName: string;
  assetType: string;
  serialNumber: string;
  location: string;
  purchaseDate: string;   // ISO 8601
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
