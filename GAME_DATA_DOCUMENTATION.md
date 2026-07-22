# 📖 Hướng Dẫn & Tài Liệu Cấu Trúc Dữ Liệu Game (SummerJam)

Tài liệu này tổng hợp toàn bộ thông tin chi tiết về cấu trúc dữ liệu, các thuộc tính, hệ thống enum và quy trình thiết kế level cho dự án **SummerJam**.

---

## 1. 👤 Khách Hàng / Nhân Vật (Person)

Nhân vật trong game đại diện cho thực khách đến nhà hàng. Mỗi nhân vật có các tính cách (Trait), điều kiện hài lòng riêng và có thể được xếp vào hàng đợi hoặc vị trí ghế ngồi.

### 🔹 PersonDataSO (`ScriptableObject`)
Nơi lưu trữ thông tin nền tảng của từng nhân vật:
* `Name` (`string`): Tên nhân vật (ví dụ: *Bob, Bi, Finn, Hieu, Josuke, Thu, Tinh, Toan, Yen...*).
* `ID` (`string`): Mã định danh duy nhất của nhân vật.
* `Trait` (`List<Trait>`): Danh sách các tính cách/đặc điểm của nhân vật.

### 🔹 Trait Enum (Tính cách & Đặc điểm)
| Giá trị Enum | Ý nghĩa | Giá trị Enum | Ý nghĩa |
| :--- | :--- | :--- | :--- |
| `Kind` | Hiền lành | `Nerd` | Mọt sách |
| `Generous` | Rộng lượng | `Quiet` | Trầm tính |
| `Talkative` | Hay nói | `Smart` | Thông minh |
| `Handsome` | Đẹp trai | `Old` | Người lớn tuổi |
| `Bad` | Ngầu / Tinh quái | `Elegant` | Thanh lịch |
| `Exhausted` | Mệt mỏi | `Chill` | Thư thái |
| `Angry` | Tức giận | `Funny` | Hài hước |
| `Vegetarian` | Ăn chay | `Female` | Nữ giới |

### 🔹 Person Component (`MonoBehaviour`)
Logic hoạt động runtime của nhân vật trên Scene:
* **Trạng thái**: `Seated` (đã ngồi vào bàn), `OutSide` (đang ở ngoài/hàng đợi), `IsHappy` (hài lòng với vị trí/món ăn xung quanh).
* **Conditions & Status**: Quản lý danh sách `ConditionStatus` (`ConditionInfo`) để cập nhật biểu tượng hài lòng (Tick xanh / O đỏ) trên giao diện.

---

## 2. 🍲 Ô Lưới & Món Ăn (Cell & Dishes)

Mọi ô trong màn chơi đều thừa kế từ lớp cơ sở **`CellDataSO`**.

### 🔹 Loại Ô (`CellType` Enum)
1. **`Block` (`Blocked.cs`)**: Ô bị chặn/không tương tác được (Hiển thị `X` màu đỏ). Đây cũng là trạng thái ô mặc định khi clear hoặc mở rộng lưới.
2. **`Seat` (`Seat.cs`)**: Ghế ngồi cho nhân vật.
   * `DefaultCanSeat = true`, `DefaultCanInteract = true`.
   * Thừa kế thuộc tính `DefaultPerson` (`Person`): Dùng cho các ghế trong hàng đợi (`WaitLineSeat`) để gán trước nhân vật mặc định (như `BobSeat`, `FinnSeat`...).
3. **`Dish` (`Dishes.cs`)**: Ô chứa món ăn hoặc thức uống.
   * `DefaultCanSeat = false`, `DefaultCanInteract = false`.
   * `Tags` (`List<Food>`): Danh sách các nhãn vị/loại thực phẩm của món ăn đó.

### 🔹 Food Enum (Nhãn Món Ăn)
* `Spicy` (Cay)
* `Cold` (Lạnh)
* `Hot` (Nóng)
* `Vegetarian` (Đồ chay)
* `Chicken` (Thịt gà)
* `Savory` (Mặn/Đậm đà)
* `Pork` (Thịt heo)
* `Beef` (Thịt bò)
* `Sweet` (Ngọt)
* `Fruit` (Trái cây)

*Danh sách món ăn có sẵn*: `BanhChung`, `Canh`, `Chuoi`, `Cua`, `DuiGa`, `Gio`, `Ngo`, `Nho`, `Soda`, `TrungOpLa` và món ăn trừu tượng `New Dishes`.

---

## 3. 🎯 Hệ Thống Điều Kiện (Conditions System)

Hệ thống điều kiện xác định khi nào một nhân vật cảm thấy hài lòng dựa trên vị trí ngồi hoặc các món ăn/nhân vật xung quanh.

### 🔹 Structure & Classes
* **`ConditionsSO`** (Lớp cơ sở): Khai báo phương thức `CheckCondition(Cell currentCell, List<Cell> adjacency)` và `GetConditionInfo()`.
* **`SingleConditionsSO`**: Điều kiện đơn lẻ gồm các tham số:
  * `Scope`: `Self` (xét chính ô đang ngồi) hoặc `Adjacent` (xét 8 ô xung quanh).
  * `FilterTarget`: `Dish` (món ăn), `Person` (người ngồi cạnh), hoặc `Cell` (vị trí ô).
  * `Comparator`: `Exact` (`==`), `AtLeast` (`>=`), `AtMost` (`<=`).
  * `Value`: Số lượng mục tiêu cần thỏa mãn.
  * `Description`: Câu mô tả hiển thị cho người chơi (ví dụ: *"Cần ngồi cạnh ít nhất 1 món cay"*).
* **`CompositeConditionsSO`**: Phức hợp kết hợp nhiều `SingleConditionsSO` thông qua các toán tử logic `And` / `Or`.
* **`ConditionInfo`**: Struct lưu trữ kết quả kiểm tra gồm `Description` và `IsSatisfied` (True/False).

---

## 4. 🗺️ Màn Chơi & Lưới (Level & Grid Config)

### 🔹 LevelConfig (`ScriptableObject`)
Quản lý tổng thể 1 màn chơi:
* `ID` (`int`): Mã số Level.
* `MoveLimit` (`int`): Số lượt di chuyển tối đa cho phép.
* `BoardGrid` (`GridConfig`): Cấu hình lưới bàn ăn chính.
* `WaitLineGrid` (`GridConfig`): Cấu hình lưới hàng đợi nhân vật.

### 🔹 GridConfig (`ScriptableObject`)
Quản lý cấu hình của một lưới cụ thể:
* `Size` (`Vector2Int`): Kích thước lưới (X: Số cột, Y: Số hàng).
* `originalCellSize` (`Vector2`): Kích thước vật lý của từng ô.
* `originalCellDistance` (`Vector2`): Khoảng cách giữa các ô.
* `PosX`, `PosY` (`float` từ 0 đến 1): Vị trí tương đối của lưới trên Viewport màn hình.
* `BaseGrid` (`Wrapper<CellDataSO>[]`): Mảng 2 chiều chứa dữ liệu từng ô.

---

## 🛠️ Công Cụ Thiết Kế Level Trực Quan (LevelConfigEditor)

Khi mở bất kỳ file `LevelConfig` nào trong Unity Inspector, bạn sẽ có các tính năng:

1. **Chỉnh sửa thông số trực tiếp**: Thay đổi `Size`, `Cell Size`, `Cell Spacing`, `Pos X/Y` ngay tại chỗ.
2. **Auto-Generate & Link**: Nếu Level mới tạo chưa có Grid, bấm nút **"Generate & Link Missing Grid Configs"** để tự động tạo folder `Assets/Data/GridData/LevelX/` chứa 2 file `LayoutX.asset` và `WaitLVX.asset` mặc định phủ sẵn ô `Blocked`.
3. **Mô phỏng & Click-to-Edit**:
   * **Clear**: Đưa ô về trạng thái `Blocked` (màu đỏ `X`).
   * **Chỗ ngồi thường**: Gán ghế ngồi cơ bản (`NozmalSeat.asset`).
   * **Món ăn (Trừu tượng)**: Gán món ăn tổng quát (Hiển thị ô màu tím không chữ).
   * **Món ăn chi tiết/**: Chọn món ăn cụ thể có nhãn (`BanhChung`, `Soda`...).
   * **Person/** (cho WaitLine): Chọn ghế gắn nhân vật tương ứng (`BobSeat`, `BiSeat`...).
