import { Component, OnInit } from "@angular/core";
import { SchoolProfileService } from "../../service/api/schoolprofile.service";
import {
    SchoolDataApiDTO, SchoolDataApiResult, AddinSchoolDataApiDTO, AddinSchoolInput,
    AddinSchoolDataApiResult, ContactPerson, AddinModule, LuuYDacThu, ThongTinServer,
    SchoolProfileInsertDTO, ApiResultBaseDO,
    SchoolProfileInsertResultDTO,
    SchoolProfileDTO,
    SchoolProfileResultDTO
} from "./SchoolProfile";

import { ConfirmationService } from 'primeng/api';

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

    displayModal: boolean = false;
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
    truongDaChon: SchoolDataApiDTO | null = null; //trường được lấy trong api
    danhSachHoSoTruong: SchoolProfileDTO[] = [];
    hoSoTruongDangXem: SchoolProfileDTO | null = null; //trường lấy trong db
    hoSoTruongDangEdit: SchoolProfileDTO | null = null;
    isLoading: boolean = false;
    newProfile: SchoolProfileInsertDTO;

    constructor(
        private truongService: SchoolProfileService,
        private confirmationService: ConfirmationService

    ) {
        this.newProfile = this.createEmptyProfile();
    }

    toggleEditMode(): void {
        if (this.selectedTab && this.hoSoTruongDangXem) {
            this.hoSoTruongDangEdit = JSON.parse(JSON.stringify(this.hoSoTruongDangXem));
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
            thoiDiemTrienKhai: null,
            soNamDungEdusoft: null,
            ngayHetHanNangCap: null,
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
            luuYXuLyDacThu: { SupportGhiChuMoHinh: '', SupportGhiChuCachHoTro: '', devGhiChu: '', saleGhiChu: '' },
            serverInfo: { nguoiQuanLy: '', thongTinChung: '', ghiChu: '' }
        }
    }

    onSelectTruong(truong: SchoolDataApiDTO): void {
        if (!truong || !truong.idTruong) {
            this.createEmptyProfile();
            return;
        }

        this.newProfile.idTruong = truong.idTruong;
        this.newProfile.maTruong = truong.maTruong;
        this.newProfile.tenTruong = truong.tenTruong;

        const input: AddinSchoolInput = {
            idTruong: truong.idTruong
        }
        this.loadAddinTruong(input);
    }

    saveProfile(): void {
        this.isLoading = true;
        const payload: SchoolProfileInsertDTO[] = [this.newProfile];
        this.truongService.addSchoolProfile(payload).subscribe(
            (respone: SchoolProfileInsertResultDTO) => {
                this.isLoading = false;
                console.log(respone);
                this.closeDialog();
            },
            (error) => {
                console.error('Lỗi khi gọi API lưu:', error);
                this.isLoading = false;
            }
        )

    }

    ngOnInit(): void {
        this.loadDanhSachTruong();
        this.loadDanhSachHoSoTruong();
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
                },
                reject: () => { }
            });
        } else {
            this.selectedTab = tabId;
        }

    }

    showModal() {
        this.displayModal = true;
        this.newProfile = this.createEmptyProfile();
        this.truongDaChon = null;
    }

    closeDialog() {
        this.displayModal = false;
        this.truongDaChon = null;
    }

    loadDanhSachTruong(): void {
        this.isLoading = true;
        this.truongService.getDanhSachTruong().subscribe(
            (response: SchoolDataApiResult) => {
                if (response.result && response.data) {
                    this.danhSachTruong = response.data;
                    console.log('Tải danh sách trường thành công:', this.danhSachTruong);
                } else {
                    console.error('API trả về lỗi:', response.message);
                }
                this.isLoading = false;
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
                    this.danhSachAddin = response.data;
                } else {
                    console.error('API trả về lỗi:', response.message);
                }
                this.isLoading = false;
            },
            (error) => {
                console.error('Lỗi khi gọi API:', error);
                this.isLoading = false;
            }
        );
    }

    loadDanhSachHoSoTruong(): void {
        this.isLoading = true;
        this.truongService.getAllSchoolProfile().subscribe(
            (respone: SchoolProfileResultDTO) => {
                console.log(respone.data);
                this.danhSachHoSoTruong = respone.data;
                if (this.danhSachHoSoTruong.length > 0) {
                    this.hoSoTruongDangXem = this.danhSachHoSoTruong[0];
                    // this.onSelectHoSoTruong(this.hoSoTruongDangXem);
                }
                this.isLoading = false;
            },
            (error) => {
                console.error('Lỗi khi gọi API:', error);
                this.isLoading = false;
            }
        )
    }

    onSelectHoSoTruong(hoSo: SchoolProfileDTO): void {
        if (this.editState[1] && this.hoSoTruongDangXem && this.hoSoTruongDangXem.idTruong !== hoSo.idTruong) {
            this.confirmationService.confirm({
                message: 'Bạn có thay đổi chưa lưu, xác nhận hhủy các thay đổi?',
                header: 'Xác nhận rời đi',
                icon: 'pi pi-exclamation-triangle',
                acceptLabel: 'Rời đi',
                rejectLabel: 'Ở lại',
                accept: () => {
                    this.cancelEditMode();
                    this.hoSoTruongDangXem = hoSo;
                    this.editState[1] = false;
                },
                reject: () => {

                }
            })
        } else {
            this.hoSoTruongDangXem = hoSo;
            if (this.editState[1]) {
                this.hoSoTruongDangEdit = JSON.parse(JSON.stringify(this.hoSoTruongDangXem));
            }
        }


    }
}