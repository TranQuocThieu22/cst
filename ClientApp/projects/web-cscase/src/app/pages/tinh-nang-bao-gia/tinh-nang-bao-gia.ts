export interface FeatureAttachment {
    fileId: string;
    fileName: string;
    fileType: string; // word, pdf, excel...
    uploadDate: Date;
}

export interface QuotationFeatureDTO {
    stt?: string;
    id: number;
    name: string;
    type: string;
    parentId: number | null;
    includedInPackage: PackageIncluded;
    detailPrice: number | null;
    featureDescription: string;
    devNote: string;
    supNote: string;
    saleNote: string;
    youtubeUrl: string;

    // Thay đổi lớn nhất ở đây:
    attachments: FeatureAttachment[];
}

export interface PackageIncluded {
    basic: boolean;
    standard: boolean;
    pro: boolean;
}