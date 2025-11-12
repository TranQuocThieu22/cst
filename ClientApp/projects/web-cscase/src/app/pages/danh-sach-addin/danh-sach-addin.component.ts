import { Component, OnInit } from "@angular/core";
import { DanhSachAddinService } from "../../service/api/addin.service";
import { DanhSachAddinDTO } from "./danh-sach-addin";

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
    // originalAddin: any = null;

    constructor(private dsAddinService: DanhSachAddinService) { }

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
    }

    enableEdit() {
        this.isEditing = true;
    }

    handleDialogClose() {
        this.isEditing = false;
        this.selectedAddin = null;
    }

    updateAddin() {
        this.isLoading = true;
        console.log("Giả lập cập nhật Addin:", this.selectedAddin);

        setTimeout(() => {
            this.isLoading = false;
            this.isEditing = false;
            this.displayDialog = false;

            // Giả lập cập nhật vào danh sách chính
            const index = this.danhSachAddin.findIndex(
                item => item.idAddin === this.selectedAddin.idAddin
            );
            if (index !== -1) {
                this.danhSachAddin[index] = { ...this.selectedAddin };
            }

            this.groupedAddinList = this.flattenToTwoLevels(this.danhSachAddin);

            this.filterTable();

            alert("✅ Cập nhật Addin thành công (giả lập)!");
        }, 1000);
    }
}
