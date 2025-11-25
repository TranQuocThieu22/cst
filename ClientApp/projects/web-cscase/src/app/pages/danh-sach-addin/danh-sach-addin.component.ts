import { Component, OnInit } from "@angular/core";
import { DanhSachAddinService } from "../../service/api/addin.service";
import { DanhSachAddinDTO, InputUpdateAddin } from "./danh-sach-addin";

import { MessageService } from 'primeng/api';

@Component({
    selector: 'danh-sach-addin',
    templateUrl: './danh-sach-addin.component.html',
    styleUrls: ['./danh-sach-addin.component.scss']
})
export class DanhSachAddinComponent implements OnInit {
    isLoading: boolean = false;
    isEditing: boolean = false;
    danhSachAddin: DanhSachAddinDTO[] = [];
    groupedAddinList: any[] = [];
    expandedGroups: { [key: string]: boolean } = {};
    searchText: string = '';
    filteredList: any[] = [];

    displayDialog: boolean = false;
    selectedAddin: any;

    wordFileName: string = '';
    pdfFileName: string = '';
    selectedWordFile: File | null = null;
    selectedPdfFile: File | null = null;

    constructor(
        private dsAddinService: DanhSachAddinService,
        private messageService: MessageService) {
    }

    ngOnInit(): void {
        this.loadAddinTruong();
    }

    loadAddinTruong() {
        this.isLoading = true;
        this.dsAddinService.fetchDanhSachAddin({}).subscribe(
            (response) => {
                this.isLoading = false;
                if (response.result && response.data) {
                    this.danhSachAddin = response.data;
                    this.groupedAddinList = this.flattenToTwoLevels(response.data);
                    this.filteredList = [...this.groupedAddinList];
                } else {
                    this.danhSachAddin = [];
                    this.groupedAddinList = [];
                    this.filteredList = [];
                }
            },
            (error) => {
                this.isLoading = false;
                console.error("Lỗi khi gọi API:", error);
            }
        );
    }

    flattenToTwoLevels(addins: any[]): any[] {
        const map = new Map<string, any>();
        const idToAddin = new Map<string, any>();

        // 1. Tạo Map tra nhanh addin theo id
        addins.forEach(a => idToAddin.set(a.idAddin, a));

        // 2. Xác định cha gốc và thêm con
        addins.forEach(addin => {
            let currentParentId = addin.idAddinParent;
            let rootParent = addin;

            // Tìm cha GỐC
            while (currentParentId && currentParentId !== '0') {
                const parent = idToAddin.get(currentParentId);
                if (!parent) break;
                rootParent = parent;
                currentParentId = parent.idAddinParent;
            }

            const rootId = rootParent.idAddin;

            // 3. Lấy hoặc tạo group (là chính đối tượng rootParent)
            if (!map.has(rootId)) {
                // Thêm mảng children vào chính rootParent
                rootParent.children = [];
                map.set(rootId, rootParent);
            }

            const group = map.get(rootId);

            // 4. Chỉ thêm vào 'children' nếu nó không phải là chính group cha
            if (addin.idAddin !== rootId) {
                // ✅ Chỉ thêm nếu chưa có trong children
                if (!group.children.find((c: any) => c.idAddin === addin.idAddin)) {
                    group.children.push(addin);
                }
            }
        });

        return Array.from(map.values());
    }

    toggleGroup(parentId: string) {
        this.expandedGroups[parentId] = !this.expandedGroups[parentId];
    }

    filterTable() {
        const keyword = this.searchText.toLowerCase().trim();
        if (!keyword) {
            this.filteredList = [...this.groupedAddinList];
            return;
        }

        this.filteredList = this.groupedAddinList
            .map(group => {
                // 1. Kiểm tra xem cha có khớp không
                const parentMatches = group.tenAddin?.toLowerCase().includes(keyword) ||
                    group.maAddin?.toLowerCase().includes(keyword);

                // 2. Lọc các con
                const filteredChildren = group.children.filter(
                    (child: any) =>
                        child.tenAddin?.toLowerCase().includes(keyword) ||
                        child.maAddin?.toLowerCase().includes(keyword)
                );

                // 3. Nếu CHA KHỚP, ta giữ lại group và TẤT CẢ con
                if (parentMatches) {
                    return group;
                }

                // 4. Nếu cha KHÔNG KHỚP, trả về group với các con ĐÃ LỌC
                return {
                    ...group,
                    children: filteredChildren
                };
            })
            .filter(group =>
                // 5. Giữ group nếu:
                //    a) Vẫn còn con (sau khi lọc hoặc là cha khớp)
                //    b) Hoặc bản thân cha khớp (cho trường hợp cha khớp nhưng không có con)
                (group.children && group.children.length > 0) ||
                (group.tenAddin?.toLowerCase().includes(keyword) ||
                    group.maAddin?.toLowerCase().includes(keyword))
            );
    }

    viewDetailAddin(group: any) {
        this.selectedAddin = JSON.parse(JSON.stringify(group));
        this.displayDialog = true;
        this.isEditing = false;

        // Reset file state
        this.wordFileName = '';
        this.pdfFileName = '';
        this.selectedWordFile = null;
        this.selectedPdfFile = null;

        // Gọi API lấy tên file đã lưu (nếu có)
        if (this.selectedAddin && this.selectedAddin.idAddin) {
            this.dsAddinService.getFileMeta(this.selectedAddin.idAddin).subscribe(
                (res) => {
                    if (res) {
                        this.wordFileName = res.fileWordName || '';
                        this.pdfFileName = res.filePdfName || '';
                    }
                },
                (err) => console.error("Không lấy được thông tin file", err)
            );
        }
    }

    enableEdit() {
        this.isEditing = true;
    }

    handleDialogClose() {
        this.isEditing = false;
        this.selectedAddin = null;
        this.wordFileName = '';
        this.pdfFileName = '';
        this.selectedWordFile = null;
        this.selectedPdfFile = null;
    }

    handleUpdateAddin() {
        if (!this.selectedAddin) return;
        this.isLoading = true;

        // 1. Chuẩn bị dữ liệu update text
        const bodyAddinUpdate: InputUpdateAddin = {
            idAddin: this.selectedAddin.idAddin,
            ghiChuAddin: this.selectedAddin.ghiChuAddin,
            ghiChuSale: this.selectedAddin.ghiChuSale,
            ghiChuDev: this.selectedAddin.ghiChuDev,
            ghiChuSupport: this.selectedAddin.ghiChuSupport,
            donGia: this.selectedAddin.donGia
        };

        // 2. Gọi API Update Text
        this.dsAddinService.updateAddin(bodyAddinUpdate).subscribe(
            (response) => {
                if (response && response.result) {
                    // 3. Nếu update text thành công -> Kiểm tra có cần upload file không
                    if (this.selectedWordFile || this.selectedPdfFile) {
                        this.uploadFilesAndFinish();
                    } else {
                        // Không có file mới -> Kết thúc
                        this.finishUpdate("Cập nhật thông tin thành công!");
                    }
                } else {
                    this.isLoading = false;
                    this.messageService.add({ severity: 'error', summary: 'Lỗi!', detail: response.message || 'Cập nhật thất bại' });
                }
            },
            (error) => {
                this.isLoading = false;
                this.messageService.add({ severity: 'error', summary: 'Lỗi!', detail: 'Có lỗi API Update' });
                console.error("Lỗi API Update:", error);
            }
        );
    }

    onFileSelect(event: any, fileType: string) {
        const fileInput = event.target as HTMLInputElement;
        if (fileInput.files && fileInput.files.length > 0) {
            const file = fileInput.files[0];
            if (fileType === 'word') {
                this.wordFileName = file.name;
                this.selectedWordFile = file;
            } else if (fileType === 'pdf') {
                this.pdfFileName = file.name;
                this.selectedPdfFile = file;
            }
        }
        fileInput.value = '';
    }

    uploadFilesAndFinish() {
        const formData = new FormData();
        formData.append("IDAddin", this.selectedAddin.idAddin);

        if (this.selectedWordFile) {
            formData.append("FileWord", this.selectedWordFile);
        }
        if (this.selectedPdfFile) {
            formData.append("FilePdf", this.selectedPdfFile);
        }

        this.dsAddinService.updateAddinFiles(formData).subscribe(
            (res) => {
                if (res && res.result) {
                    this.finishUpdate("Cập nhật thông tin thành công!");
                } else {
                    this.finishUpdate("Cập nhật thông tin thành công, lỗi upload file.");
                }
            },
            (err) => {
                console.error("Lỗi upload file", err);
                this.finishUpdate("Cập nhật thông tin thành công, lỗi upload file.");
            }
        );
    }
    finishUpdate(message: string) {
        this.isLoading = false;
        this.messageService.add({ severity: 'success', summary: 'Thông báo', detail: message });
        this.displayDialog = false;
        this.isEditing = false;
        this.selectedAddin = null;
        // Reload lại danh sách để cập nhật dữ liệu mới nhất
        this.loadAddinTruong();
    }

    onDownloadFile(fileType: 'word' | 'pdf') {
        if (!this.selectedAddin || !this.selectedAddin.idAddin) return;

        // Kiểm tra xem có tên file chưa (nghĩa là có file để tải)
        const fileName = fileType === 'word' ? this.wordFileName : this.pdfFileName;
        if (!fileName) {
            this.messageService.add({ severity: 'warn', summary: 'Chú ý', detail: 'Chưa có file được lưu để tải về.' });
            return;
        }

        this.dsAddinService.downloadFile(this.selectedAddin.idAddin, fileType).subscribe(
            (blob) => {
                // Tạo url ảo từ blob
                const url = window.URL.createObjectURL(blob);

                // Tạo thẻ a ẩn để click tải về
                const a = document.createElement('a');
                a.href = url;
                a.download = fileName; // Đặt tên file khi tải về
                document.body.appendChild(a);
                a.click();

                // Dọn dẹp
                document.body.removeChild(a);
                window.URL.revokeObjectURL(url);
            },
            (error) => {
                console.error("Download error:", error);
                this.messageService.add({ severity: 'error', summary: 'Lỗi', detail: 'Không thể tải file.' });
            }
        );
    }

    // Thêm hàm này vào class DanhSachAddinComponent

    onDownloadFileInTable(item: any, fileType: 'word' | 'pdf') {
        if (!item || !item.idAddin) return;

        // Lấy tên file từ đối tượng dòng hiện tại
        const fileName = fileType === 'word' ? item.fileWordName : item.filePdfName;

        if (!fileName) {
            this.messageService.add({ severity: 'warn', summary: 'Thông báo', detail: 'File không tồn tại.' });
            return;
        }

        this.isLoading = true; // Bật loading nhẹ
        this.dsAddinService.downloadFile(item.idAddin, fileType).subscribe(
            (blob) => {
                this.isLoading = false;
                const url = window.URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = url;
                a.download = fileName;
                document.body.appendChild(a);
                a.click();
                document.body.removeChild(a);
                window.URL.revokeObjectURL(url);
            },
            (error) => {
                this.isLoading = false;
                console.error("Download error:", error);
                this.messageService.add({ severity: 'error', summary: 'Lỗi', detail: 'Không thể tải file.' });
            }
        );
    }
}
