import { Component, OnInit } from "@angular/core";
import { SchoolProfileService } from "../../service/api/schoolprofile.service";
import {
    SchoolDataApiDTO, SchoolDataApiResult, AddinSchoolDataApiDTO, AddinSchoolInput,
    AddinSchoolDataApiResult, ContactPerson, SchoolProfileInsertDTO,
    SchoolProfileDTO, SchoolProfileResultDTO, SchoolProfileInsertResultDTO
} from "./SchoolProfile";

import { ConfirmationService, MessageService } from 'primeng/api';
import { Subscription } from "rxjs";
import { HttpClient } from "@angular/common/http";

@Component({
    selector: "danh-sach-truong",
    templateUrl: './danh-sach-truong.component.html',
    styleUrls: ['./danh-sach-truong.component.scss']
})
export class DanhSachTruongComponent implements OnInit {
    tabs = [
        { id: 1, label: 'Thông tin trường' },
        { id: 2, label: 'Danh sách Addin' },
        { id: 3, label: 'Thống kê case TFS' },
        { id: 4, label: 'Đặc thù trường' },
        { id: 5, label: 'Thông tin server' },
    ];

    selectedTab = 1;
    editState: { [tabId: number]: boolean } = {
        1: false,
        2: false,
        3: false,
        4: false,
        5: false
    };
    danhSachAddin: AddinSchoolDataApiDTO[] = [];
    danhSachTruong: SchoolDataApiDTO[] = [];
    danhSachHoSoTruong: SchoolProfileDTO[] = [];
    hoSoTruongDangXem: SchoolProfileDTO | null = null;
    hoSoTruongDangEdit: SchoolProfileDTO | null = null;
    apiDataDangXem: SchoolDataApiDTO | null = null;
    isLoading: boolean = false;
    selectedAddinForDetail: AddinSchoolDataApiDTO | null = null;

    groupedAddinList: any[] = [];
    expandedGroups: { [key: string]: boolean } = {};
    searchText: string = '';
    filteredList: any[] = [];
    displayAddinDialog: boolean = false;

    private ssoSub: Subscription | null = null;
    isSsoLoading: boolean = false;

    constructor(
        private truongService: SchoolProfileService,
        private confirmationService: ConfirmationService,
        private messageService: MessageService,
        private http: HttpClient
    ) {
    }

    toggleEditMode(): void {
        if (this.selectedTab && this.hoSoTruongDangXem) {
            this.hoSoTruongDangEdit = JSON.parse(JSON.stringify(this.hoSoTruongDangXem));
            const emptyContactPerson = { hoTen: '', dienThoai: '', email: '' };
            const emptyDanhSachAddin = { ghiChuSale: '', ghiChuSupport: '', ghiChuDev: '' };
            const emptyLuuYDacThu = { supportGhiChuMoHinh: '', supportGhiChuCachHoTro: '', devGhiChu: '', saleGhiChu: '' };
            const emptyThongTinServer = { nguoiQuanLy: '', thongTinChung: '', ghiChu: '' };
            if (this.hoSoTruongDangEdit) {
                this.hoSoTruongDangEdit.hieuTruong = { ...emptyContactPerson, ...this.hoSoTruongDangEdit.hieuTruong };
                this.hoSoTruongDangEdit.hieuPho = { ...emptyContactPerson, ...this.hoSoTruongDangEdit.hieuPho };
                this.hoSoTruongDangEdit.truongPhongDaoTao = { ...emptyContactPerson, ...this.hoSoTruongDangEdit.truongPhongDaoTao };
                this.hoSoTruongDangEdit.truongPhongKhaoThi = { ...emptyContactPerson, ...this.hoSoTruongDangEdit.truongPhongKhaoThi };
                this.hoSoTruongDangEdit.truongPhongTaiVu = { ...emptyContactPerson, ...this.hoSoTruongDangEdit.truongPhongTaiVu };
                this.hoSoTruongDangEdit.admin = { ...emptyContactPerson, ...this.hoSoTruongDangEdit.admin };
                this.hoSoTruongDangEdit.luuYXuLyDacThu = { ...emptyLuuYDacThu, ...this.hoSoTruongDangEdit.luuYXuLyDacThu };
                this.hoSoTruongDangEdit.danhSachAddin = { ...emptyDanhSachAddin, ...this.hoSoTruongDangEdit.danhSachAddin };
                this.hoSoTruongDangEdit.serverInfo = { ...emptyThongTinServer, ...this.hoSoTruongDangEdit.serverInfo };
            }

            this.editState[this.selectedTab] = true;
        }
    }

    cancelEditMode(): void {
        if (this.selectedTab) {
            this.editState[this.selectedTab] = false;
            this.hoSoTruongDangEdit = null;
        }
    }

    createEmptyProfile(): SchoolProfileInsertDTO {
        const emptyContact: ContactPerson = { hoTen: '', dienThoai: '', email: '' };
        return {
            idTruong: '',
            maTruong: '',
            tenTruong: '',
            diaChiTruong: '',
            hieuTruong: { ...emptyContact },
            hieuPho: { ...emptyContact },
            truongPhongDaoTao: { ...emptyContact },
            truongPhongKhaoThi: { ...emptyContact },
            truongPhongTaiVu: { ...emptyContact },
            admin: { ...emptyContact },
            ghiChuKinhDoanh: '',
            ghiChuKyThuat: '',
            ghiChuChamSoc: '',
            danhSachAddin: { ghiChuDev: '', ghiChuSupport: '', ghiChuSale: '' },
            luuYXuLyDacThu: { supportGhiChuMoHinh: '', supportGhiChuCachHoTro: '', devGhiChu: '', saleGhiChu: '' },
            serverInfo: { nguoiQuanLy: '', thongTinChung: '', ghiChu: '' }
        }
    }

    ngOnInit(): void {
        this.loadDanhSachTruong();
    }

    selectTab(tabId: number) {
        if (this.editState[this.selectedTab] && this.selectedTab !== tabId) {
            this.confirmationService.confirm({
                message: 'Bạn có thay đổi chưa lưu. Bạn có chắc chắn muốn rời đi và hủy các thay đổi này không?',
                header: 'Xác nhận rời đi',
                icon: 'pi pi-exclamation-triangle',
                acceptLabel: 'Rời đi',
                rejectLabel: 'Ở lại',
                accept: () => {
                    this.editState[this.selectedTab] = false;
                    this.cancelEditMode();
                    this.selectedTab = tabId;
                    if (tabId === 2) {
                        this.loadDataForTabAddin();
                    }
                },
                reject: () => { }
            });
        } else if (this.selectedTab !== tabId) { // Thêm điều kiện kiểm tra để tránh tải lại khi bấm vào tab đang active
            this.selectedTab = tabId;
            if (tabId === 2) {
                this.loadDataForTabAddin();
            }
        } else {
            this.selectedTab = tabId;

        }
    }

    loadDanhSachTruong(): void {
        this.isLoading = true;
        this.truongService.getDanhSachTruong().subscribe(
            (response: SchoolDataApiResult) => {
                if (response.result && response.data) {
                    this.danhSachTruong = response.data;
                    this.loadDanhSachHoSoTruong();
                } else {
                    console.error('API trả về lỗi:', response.message);
                }
            },
            (error) => {
                console.error('Lỗi khi gọi API:', error);
                this.isLoading = false;
            }
        );
    }

    loadAddinTruong(idTruong: AddinSchoolInput): void {
        this.isLoading = true;
        this.truongService.fetchDanhSachAddinTruong(idTruong).subscribe(
            (response: AddinSchoolDataApiResult) => {
                if (response.result && response.data) {
                    // console.log("addin: ", response.data)
                    this.danhSachAddin = response.data;
                    this.groupedAddinList = this.flattenToTwoLevels(response.data);
                    this.filteredList = [...this.groupedAddinList];
                } else {
                    this.danhSachAddin = [];
                    this.groupedAddinList = [];
                    this.filteredList = [];
                    console.error('API trả về lỗi:', response.message);
                }
                this.isLoading = false;
            },
            (error) => {
                this.danhSachAddin = [];
                this.groupedAddinList = [];
                this.filteredList = [];
                console.error('Lỗi khi gọi API:', error);
                this.isLoading = false;
            }
        );
    }

    loadDanhSachHoSoTruong(isReload: boolean = false): void {
        this.isLoading = true;
        this.truongService.getAllSchoolProfile().subscribe(
            (respone: SchoolProfileResultDTO) => {
                this.danhSachHoSoTruong = respone.data;
                if (isReload) {
                    if (this.danhSachHoSoTruong.length > 0) {
                        this.hoSoTruongDangXem = this.danhSachHoSoTruong[0];
                    }
                    this.isLoading = false;
                } else {
                    this.syncDanhSachTruong();
                }
            },
            (error) => {
                console.error('Lỗi khi gọi API:', error);
                this.isLoading = false;
            }
        )

    }

    syncDanhSachTruong(): void {
        const dbSchoolIds = new Set(this.danhSachHoSoTruong.map(p => p.idTruong)); //trường db
        const truongCanThemMoi: SchoolProfileInsertDTO[] = [];

        for (const apiSchool of this.danhSachTruong) { //lặp qua danh sách trường trong api
            if (!dbSchoolIds.has(apiSchool.idTruong)) {
                const newProfile = this.createEmptyProfile();
                newProfile.idTruong = apiSchool.idTruong;
                newProfile.maTruong = apiSchool.maTruong;
                newProfile.tenTruong = apiSchool.tenTruong;
                truongCanThemMoi.push(newProfile);
            }
        }
        if (truongCanThemMoi.length > 0) {
            this.truongService.addSchoolProfile(truongCanThemMoi).subscribe(
                (respone: SchoolProfileInsertResultDTO) => {
                    this.messageService.add({ severity: 'info', summary: 'Đồng bộ', detail: `Đã tự động thêm ${truongCanThemMoi.length} hồ sơ trường mới.` });
                    this.loadDanhSachHoSoTruong(true);
                },
                (error) => {
                    this.messageService.add({ severity: 'error', summary: 'Lỗi đồng bộ', detail: 'Không thể tự động thêm trường mới.' });
                    this.isLoading = false;
                }
            );
        } else {
            if (this.danhSachHoSoTruong.length > 0) {
                this.hoSoTruongDangXem = this.danhSachHoSoTruong[0];
            }
            this.isLoading = false;
        }
    }

    onSelectHoSoTruong(hoSo: SchoolProfileDTO): void {
        if (this.editState[this.selectedTab] && this.hoSoTruongDangXem && this.hoSoTruongDangXem.idTruong !== hoSo.idTruong) {
            this.confirmationService.confirm({
                message: 'Bạn có thay đổi chưa lưu, xác nhận hhủy các thay đổi?',
                header: 'Xác nhận rời đi',
                icon: 'pi pi-exclamation-triangle',
                acceptLabel: 'Rời đi',
                rejectLabel: 'Ở lại',
                accept: () => {
                    this.cancelEditMode();
                    this.hoSoTruongDangXem = hoSo;
                    this.editState[this.selectedTab] = false;
                    if (this.selectedTab === 2) {
                        this.loadDataForTabAddin();
                    }
                },
                reject: () => {

                }
            })
        } else {
            this.hoSoTruongDangXem = hoSo;
            this.apiDataDangXem = this.danhSachTruong.find(
                apiSchool => apiSchool.idTruong === hoSo.idTruong
            ) || null;
            if (this.selectedTab === 2) {
                this.loadDataForTabAddin();
            }
            if (this.editState[this.selectedTab]) {
                this.toggleEditMode();
            }
        }
    }

    tinhSoNamSuDung(thoiDiemTrienKhaiStr: string | undefined | null): number | string {
        if (!thoiDiemTrienKhaiStr) {
            return '';
        }

        const ngayTrienKhai = new Date(thoiDiemTrienKhaiStr);
        if (isNaN(ngayTrienKhai.getTime())) {
            return '';
        }

        const homNay = new Date();
        const duration = homNay.getTime() - ngayTrienKhai.getTime();
        const totalDays = duration / (1000 * 60 * 60 * 24);

        const soNam = Math.floor(totalDays / 365.25);

        return soNam > 0 ? `${soNam} năm` : 0;
    }

    saveTabInfo(): void {
        if (this.hoSoTruongDangEdit) {
            this.isLoading = true;
            const { maTruong, idTruong, tenTruong, ...updateData } = this.hoSoTruongDangEdit;
            this.truongService.updateOneSchoolProfile(idTruong, updateData).subscribe(
                (respone) => {
                    const savedProfile = JSON.parse(JSON.stringify(this.hoSoTruongDangEdit));
                    const index = this.danhSachHoSoTruong.findIndex(
                        (hoSo) => hoSo.idTruong === savedProfile.idTruong
                    );
                    if (index !== -1) {
                        this.danhSachHoSoTruong[index] = savedProfile;
                    }
                    this.hoSoTruongDangXem = this.danhSachHoSoTruong[index];
                    this.editState[this.selectedTab] = false;
                    this.hoSoTruongDangEdit = null;
                    this.messageService.add({ severity: 'success', summary: 'Thành công', detail: 'Đã lưu hồ sơ trường' });
                    this.isLoading = false;
                },
                (error) => {
                    console.log('Lỗi khi gọi API lưu:', error);
                    this.isLoading = false;
                }
            );
        }
    }

    loadDataForTabAddin(): void {
        this.selectedAddinForDetail = null;
        this.searchText = '';

        if (this.hoSoTruongDangXem) { // Chỉ tải khi đã có trường được chọn
            const input: AddinSchoolInput = {
                idTruong: this.hoSoTruongDangXem.idTruong
            }
            this.loadAddinTruong(input);
        } else {
            // Nếu không có trường nào được chọn, xóa dữ liệu cũ
            this.groupedAddinList = [];
            this.filteredList = [];
            this.danhSachAddin = [];
        }
    }

    onAddinRowClick(data: AddinSchoolDataApiDTO): void {
        this.selectedAddinForDetail = data;
        this.displayAddinDialog = true;
        if (this.editState) {
            this.cancelEditMode()
        }
    }

    navigateToOtherApp() {
        this.http.get<any>('/api/sso/sso-cst-url').subscribe(res => {
            window.open(res.autoLoginUrl, "_blank");
        });
    }

    onCloseAddinDialog(): void {
        this.displayAddinDialog = false;
        this.selectedAddinForDetail = null;
        if (this.editState[this.selectedTab]) {
            this.editState[this.selectedTab] = false;
            this.hoSoTruongDangEdit = null;
        }
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

    ngOnDestroy() {
        if (this.ssoSub) {
            this.ssoSub.unsubscribe();
        }
    }
}

