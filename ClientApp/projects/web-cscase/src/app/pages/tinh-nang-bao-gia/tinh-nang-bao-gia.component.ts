import { Component, OnInit } from "@angular/core";
import { TreeNode, SelectItem, MessageService, ConfirmationService } from 'primeng/api';
import { QuotationFeatureService } from "../../service/api/quotationFeature.service";
import { QuotationFeatureDTO, FeatureAttachment } from "./tinh-nang-bao-gia";

@Component({
    selector: 'tinh-nang-bao-gia',
    templateUrl: './tinh-nang-bao-gia.component.html',
    styleUrls: ['./tinh-nang-bao-gia.component.scss'],
    providers: [MessageService, ConfirmationService]
})
export class TinhNangBaoGiaComponent implements OnInit {

    constructor(
        private quotationFeatureService: QuotationFeatureService,
        private messageService: MessageService,
        private confirmationService: ConfirmationService
    ) { }

    loading: boolean = false;

    // --- BIẾN LƯU TỔNG TOÀN CỤC ---
    grandTotalBasic: number = 0;
    grandTotalStandard: number = 0;
    grandTotalPro: number = 0;

    // --- BIẾN HIỂN THỊ CÂY ---
    treeData: TreeNode[] = [];
    allOriginalData: QuotationFeatureDTO[] = [];

    // --- BIẾN DIALOG & FORM ---
    displayDialog: boolean = false; // Dialog xem chi tiết
    dialogFormVisible: boolean = false; // Dialog Thêm/Sửa
    dialogHeader: string = '';
    isEditMode: boolean = false;

    selectedFeature: any = null; // Dùng cho xem chi tiết

    // Dữ liệu Form
    formData: QuotationFeatureDTO = this.resetForm();

    // Dropdown options
    featureTypes: SelectItem[] = [
        { label: 'Nhóm phần mềm', value: 'group' },
        { label: 'Module', value: 'module' },
        { label: 'Chức năng', value: 'feature' }
    ];

    // Biến tạm để binding với Radio Button
    selectedPackageLevel: string = '';

    parentOptions: SelectItem[] = [];

    // --- BIẾN QUẢN LÝ FILE (MỚI) ---
    // Danh sách các file mới được user chọn từ máy tính (chờ upload)
    uploadedFiles: File[] = [];
    // Danh sách ID các file cũ mà user muốn xóa
    filesToDelete: string[] = [];

    ngOnInit(): void {
        this.loadDanhSachTinhNangBaoGia();
    }

    // --- 1. LOAD DATA ---
    loadDanhSachTinhNangBaoGia() {
        this.loading = true;
        this.quotationFeatureService.getList().subscribe({
            next: (response: QuotationFeatureDTO[]) => {
                this.allOriginalData = response;
                this.treeData = this.buildTree(response, null);
                const totals = this.calculateTotals(this.treeData);
                this.grandTotalBasic = totals.basic;
                this.grandTotalStandard = totals.standard;
                this.grandTotalPro = totals.pro;
                this.loading = false;
            },
            error: (err) => {
                console.error(err);
                this.messageService.add({ severity: 'error', summary: 'Lỗi', detail: 'Không tải được dữ liệu' });
                this.loading = false;
            }
        });
    }

    // --- 2. XỬ LÝ FORM THÊM / SỬA ---

    resetForm(): QuotationFeatureDTO {
        return {
            id: 0,
            name: '',
            type: 'group',
            parentId: null,
            detailPrice: null,
            includedInPackage: { basic: false, standard: false, pro: false },
            featureDescription: '',
            devNote: '',
            saleNote: '',
            supNote: '',
            youtubeUrl: '',
            attachments: [] // Khởi tạo mảng rỗng
        } as QuotationFeatureDTO;
    }

    // Mở Dialog THÊM MỚI
    openAddDialog(type: 'group' | 'module' | 'feature' = 'group', parent: any = null) {
        this.isEditMode = false;
        this.formData = this.resetForm();

        // Reset lựa chọn gói hỗ trợ
        this.selectedPackageLevel = '';

        // Thiết lập loại và tiêu đề
        this.formData.type = type;
        this.dialogHeader = 'Thêm mới';

        // Xử lý Parent
        if (parent) {
            this.formData.parentId = parent.id;
        } else {
            this.formData.parentId = null;
            this.updateParentOptions();
        }

        // Reset trạng thái file
        this.uploadedFiles = [];
        this.filesToDelete = [];

        this.dialogFormVisible = true;
    }

    // Mở Dialog CHỈNH SỬA
    openEditDialog(row: any) {
        this.isEditMode = true;
        this.dialogHeader = `Chỉnh sửa tính năng`;

        // Deep copy object để tránh sửa trực tiếp vào bảng
        this.formData = JSON.parse(JSON.stringify(row));

        // Đảm bảo các object con tồn tại
        if (!this.formData.includedInPackage) {
            this.formData.includedInPackage = { basic: false, standard: false, pro: false };
        }
        if (!this.formData.attachments) {
            this.formData.attachments = [];
        }

        // MAP DỮ LIỆU TỪ BOOLEAN SANG BIẾN STRING CHO RADIO BUTTON
        this.selectedPackageLevel = '';
        if (this.formData.includedInPackage) {
            if (this.formData.includedInPackage.pro) {
                this.selectedPackageLevel = 'pro';
            } else if (this.formData.includedInPackage.standard) {
                this.selectedPackageLevel = 'standard';
            } else if (this.formData.includedInPackage.basic) {
                this.selectedPackageLevel = 'basic';
            }
        }

        this.updateParentOptions();

        // Reset trạng thái file
        this.uploadedFiles = [];
        this.filesToDelete = [];

        this.dialogFormVisible = true;
    }

    // --- 3. XỬ LÝ FILE (LOGIC MỚI) ---

    // Khi user chọn file từ máy tính (Input Change)
    onFilesSelected(event: any) {
        if (event.target.files && event.target.files.length > 0) {
            for (let i = 0; i < event.target.files.length; i++) {
                const file = event.target.files[i];
                // Có thể validate size/type ở đây nếu cần
                this.uploadedFiles.push(file);
            }
        }
        // Reset input để có thể chọn lại cùng 1 file nếu lỡ xóa nhầm
        event.target.value = '';
    }

    // Xóa file khỏi danh sách CHỜ upload (File chưa gửi lên server)
    removeUploadedFile(index: number) {
        this.uploadedFiles.splice(index, 1);
    }

    // Đánh dấu file ĐÃ CÓ trong DB để xóa (Khi user bấm nút xóa file cũ)
    markFileForDeletion(fileId: string) {
        // Thêm vào danh sách cần xóa gửi về server
        this.filesToDelete.push(fileId);

        // Ẩn khỏi giao diện hiện tại
        if (this.formData.attachments) {
            this.formData.attachments = this.formData.attachments.filter(f => f.fileId !== fileId);
        }
    }

    // --- 4. LƯU DỮ LIỆU (SAVE) ---
    save() {
        // Validation
        if (!this.formData.name) {
            this.messageService.add({ severity: 'warn', summary: 'Cảnh báo', detail: 'Tên không được để trống' });
            return;
        }

        // Validation ParentId cho Module và Feature
        if (this.formData.type === 'module' && !this.formData.parentId) {
            this.messageService.add({ severity: 'warn', summary: 'Cảnh báo', detail: 'Vui lòng chọn Nhóm cha' });
            return;
        }
        if (this.formData.type === 'feature' && !this.formData.parentId) {
            this.messageService.add({ severity: 'warn', summary: 'Cảnh báo', detail: 'Vui lòng chọn Module cha' });
            return;
        }

        if (this.formData.type === 'feature') {
            if (!this.selectedPackageLevel) {
                this.messageService.add({
                    severity: 'warn',
                    summary: 'Cảnh báo',
                    detail: 'Vui lòng chọn một gói hỗ trợ (Basic, Standard hoặc Pro)'
                });
                return;
            }
            this.formData.includedInPackage = { basic: false, standard: false, pro: false };

            // Bật true cho gói được chọn
            if (this.selectedPackageLevel === 'basic') this.formData.includedInPackage.basic = true;
            if (this.selectedPackageLevel === 'standard') this.formData.includedInPackage.standard = true;
            if (this.selectedPackageLevel === 'pro') this.formData.includedInPackage.pro = true;
        }

        // --- TẠO FORMDATA ---
        const payload = new FormData();

        // 1. Append các dữ liệu cơ bản
        for (const key of Object.keys(this.formData)) {
            const value = (this.formData as any)[key];

            // Bỏ qua attachments (vì ta xử lý file riêng) và null/undefined
            if (key === 'attachments' || value === null || value === undefined) continue;

            if (key === 'includedInPackage') {
                // Xử lý object con
                payload.append('IncludedInPackage.Basic', value.basic.toString());
                payload.append('IncludedInPackage.Standard', value.standard.toString());
                payload.append('IncludedInPackage.Pro', value.pro.toString());
            } else {
                payload.append(key, value.toString());
            }
        }

        // 2. Append file mới (UploadFiles)
        // Tên 'UploadFiles' phải trùng với Property trong C# DTO
        this.uploadedFiles.forEach(file => {
            payload.append('UploadFiles', file);
        });

        // 3. Append danh sách ID file cần xóa (RemoveFileIds)
        // Tên 'RemoveFileIds' phải trùng với Property trong C# DTO
        this.filesToDelete.forEach(id => {
            payload.append('RemoveFileIds', id);
        });

        // --- GỌI API ---
        const apiCall = this.isEditMode
            ? this.quotationFeatureService.update(payload)
            : this.quotationFeatureService.add(payload);

        apiCall.subscribe({
            next: (res) => {
                this.messageService.add({ severity: 'success', summary: 'Thành công', detail: 'Đã lưu dữ liệu thành công' });
                this.dialogFormVisible = false;
                this.loadDanhSachTinhNangBaoGia(); // Reload lại bảng
            },
            error: (err) => {
                console.error(err);
                this.messageService.add({ severity: 'error', summary: 'Lỗi', detail: 'Có lỗi xảy ra khi lưu dữ liệu' });
            }
        });
    }

    // --- 5. CÁC HÀM HỖ TRỢ KHÁC ---

    onTypeChange() {
        this.formData.parentId = null;
        this.updateParentOptions();
    }

    updateParentOptions() {
        this.parentOptions = [];
        if (this.formData.type === 'module') {
            this.parentOptions = this.allOriginalData
                .filter(x => x.type === 'group')
                .map(x => ({ label: x.name, value: x.id }));
        } else if (this.formData.type === 'feature') {
            this.parentOptions = this.allOriginalData
                .filter(x => x.type === 'module')
                .map(x => ({ label: x.name, value: x.id }));
        }
    }

    downloadFile(fileId: string, fileName: string) {
        this.quotationFeatureService.downloadFile(fileId).subscribe({
            next: (blob) => {
                const url = window.URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = url;
                a.download = fileName;
                a.click();
                window.URL.revokeObjectURL(url);
            },
            error: () => {
                this.messageService.add({ severity: 'error', summary: 'Lỗi', detail: 'Không thể tải file' });
            }
        });
    }

    // Trong file .ts

    onDelete(row: any) {
        this.confirmationService.confirm({
            message: `Bạn có chắc chắn muốn xóa "${row.name}"?`,
            header: 'Xác nhận xóa',
            icon: 'pi pi-exclamation-triangle',
            acceptLabel: 'Đồng ý',
            rejectLabel: 'Hủy',
            acceptButtonStyleClass: 'p-button-danger',
            rejectButtonStyleClass: 'p-button-text',
            accept: () => {
                this.quotationFeatureService.delete(row.id).subscribe({
                    next: () => {
                        this.messageService.add({ severity: 'success', summary: 'Thành công', detail: 'Đã xóa thành công' });
                        this.loadDanhSachTinhNangBaoGia();
                    },
                    error: (err) => {
                        console.error(err);

                        // --- XỬ LÝ HIỂN THỊ LỖI TỪ BACKEND ---
                        // Backend trả về BadRequest("Chuỗi thông báo lỗi") -> err.error sẽ là chuỗi đó
                        // Hoặc Backend trả về BadRequest({message: "..."}) -> err.error.message

                        let errorMsg = 'Xóa thất bại. Có lỗi xảy ra.';

                        if (err.status === 400) {
                            // Lấy thông báo lỗi cụ thể từ Backend
                            errorMsg = typeof err.error === 'string' ? err.error : (err.error?.title || 'Không thể xóa mục này do ràng buộc dữ liệu.');
                        }

                        this.messageService.add({
                            severity: 'error',
                            summary: 'Không thể xóa',
                            detail: errorMsg,
                            life: 5000 // Hiện lâu hơn chút để user kịp đọc
                        });
                    }
                });
            }
        });
    }

    onRowSelect(node: TreeNode) {
        if (node.data.type === 'group') return;
        this.selectedFeature = node.data;
        this.displayDialog = true;
    }

    // --- HÀM XỬ LÝ CÂY VÀ TÍNH TOÁN ---
    buildTree(items: QuotationFeatureDTO[], parentId: number | null, prefix: string = ''): TreeNode[] {

        // 1. Lấy danh sách các item thuộc cấp hiện tại
        const currentLevelItems = items.filter(i => {
            if (parentId === null) {
                return i.parentId === null;
            }
            return i.parentId === parentId;
        });

        // 2. Map qua từng item để tính STT và tìm con
        return currentLevelItems.map((item, index) => {
            // Tính số thứ tự hiện tại (index bắt đầu từ 0 nên phải +1)
            const displayIndex = index + 1;

            // Tạo chuỗi STT: Nếu có prefix (VD: "1") thì thành "1.1", nếu không thì thành "1"
            const currentSTT = prefix ? `${prefix}.${displayIndex}` : `${displayIndex}`;

            // Gán vào data để hiển thị ra HTML
            item.stt = currentSTT;

            // Gọi đệ quy cho con, truyền currentSTT xuống làm prefix
            const children = this.buildTree(items, item.id, currentSTT);

            return {
                data: item,
                children: children,
                leaf: children.length === 0,
                expanded: true
            };
        });
    }

    calculateTotals(nodes: TreeNode[]): { basic: number, standard: number, pro: number } {
        let levelBasic = 0;
        let levelStandard = 0;
        let levelPro = 0;

        nodes.forEach(node => {
            let nodeBasic = 0;
            let nodeStandard = 0;
            let nodePro = 0;

            if (node.children && node.children.length > 0) {
                const childTotals = this.calculateTotals(node.children);
                nodeBasic = childTotals.basic;
                nodeStandard = childTotals.standard;
                nodePro = childTotals.pro;
            } else {
                const price = node.data.detailPrice || 0;
                const pkg = node.data.includedInPackage || {};
                if (pkg.basic) nodeBasic += price;
                if (pkg.standard || pkg.basic) nodeStandard += price;
                nodePro += price;
            }

            node.data.totalBasic = nodeBasic;
            node.data.totalStandard = nodeStandard;
            node.data.totalPro = nodePro;

            levelBasic += nodeBasic;
            levelStandard += nodeStandard;
            levelPro += nodePro;
        });

        return { basic: levelBasic, standard: levelStandard, pro: levelPro };
    }
}