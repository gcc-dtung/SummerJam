# Dự Án Game: Wanna Sit Here? — Ai Sẽ Ngồi Ở Đâu? 🪑🍽️

[<img src="https://github.com/user-attachments/assets/dabe4f53-fcfb-4e3f-bdd8-a8ebdddf2423" alt="CLB Nhà Sáng Tạo Game PTIT Logo" width="30"/> **CLB Nhà Sáng Tạo Game PTIT**](https://www.facebook.com/gamecreatorsclub)

**Thời gian phát triển:** 07/2026 - 08/2026

**Vai trò:** Unity Developer

**Xem gameplay tại đây:**

<!--
Để GitHub hiển thị video player giống ảnh demo:
1. Mở README.md trên GitHub bằng Edit.
2. Kéo-thả file Docs/Demo/wanna-sit-here-demo-under-10mb.mp4 vào đúng vị trí này.
3. GitHub sẽ tạo link dạng https://github.com/user-attachments/assets/...
4. Dán link đó ở ngay dòng trống bên dưới comment này.
-->

<!-- Dán GitHub video asset URL ở đây để hiện player:
https://github.com/user-attachments/assets/PASTE_VIDEO_ASSET_URL_HERE
-->

Video dự phòng: [Video demo - Wanna Sit Here?](Docs/Demo/wanna-sit-here-demo-under-10mb.mp4)

---

## Tổng quan dự án

**Wanna Sit Here?** là một tựa game giải đố kéo-thả 2D lấy bối cảnh quanh bàn ăn. Mỗi vị khách đều có sở thích riêng: muốn hoặc không muốn ngồi cạnh một kiểu người nhất định, thích một món ăn cụ thể, hoặc chỉ hài lòng khi được xếp vào đúng vị trí.

Nhiệm vụ của người chơi là quan sát các gợi ý, sắp xếp toàn bộ khách từ hàng chờ vào những chiếc ghế quanh bàn và khiến tất cả cùng hài lòng trước khi hết lượt. Càng về sau, số lượng nhân vật, món ăn và điều kiện kết hợp càng tăng, tạo nên những câu đố logic nhiều lớp nhưng vẫn gần gũi và vui nhộn.

---

## Các tính năng nổi bật

- **Giải đố sắp xếp chỗ ngồi:** Kéo-thả và hoán đổi nhân vật giữa hàng chờ với các ghế trên bàn chơi.
- **Hệ thống điều kiện linh hoạt:** Nhân vật có thể thích hoặc tránh món ăn, đặc điểm của người ngồi cạnh, hay một vị trí cụ thể
- **Giới hạn lượt và trạng thái thắng/thua:** Người chơi chiến thắng khi tất cả khách đã vào bàn và đều hài lòng; hết lượt trước khi giải xong sẽ dẫn đến thất bại.
- **Ba loại Booster:** Hoàn tác nước đi (`Undo`), loại bỏ điều kiện của nhân vật (`Remove`) và cộng thêm lượt (`More Move`).
- **Kinh tế và cửa hàng:** Tích hợp Gold, Gem, vật phẩm mua trong shop và phần thưởng sau mỗi màn.
- **Phần thưởng hằng ngày:** Theo dõi tiến trình nhận thưởng theo tuần và lưu trạng thái giữa các phiên chơi.
- **Âm thanh, chuyển cảnh và VFX:** BGM/SFX riêng cho menu và gameplay; animation UI, phản hồi kéo-thả và hiệu ứng thắng/thua được xây dựng bằng `PrimeTween` và UI Particle.
- **Tối ưu cho thiết bị di động:** Hỗ trợ Input System, Safe Area và giao diện màn hình dọc.



## Công nghệ sử dụng

`C#`, `Unity Engine`, `PrimeTween`

**Mã nguồn:** [GitHub - Wanna Sit Here?](https://github.com/gcc-dtung/SummerJam)

---

## Cài đặt và chạy dự án

1. Clone repository:

   ```bash
   git clone https://github.com/gcc-dtung/SummerJam.git
   ```

2. Mở thư mục dự án bằng **Unity Hub** với phiên bản **Unity 6000.3.18f1**.
3. Chờ Unity tải package và import toàn bộ asset.
4. Mở scene `Assets/Scenes/Level.unity`.
5. Nhấn **Play** để bắt đầu.

---

## Credits

- **Unity Development:** [Nguyễn Đức Tùng](https://github.com/gcc-dtung)
- **Unity Development:** [Đoàn Hải Đăng](https://github.com/Dagnarion)
- **Game Designer:** [Nguyễn Đức Toàn](https://github.com/duktofn)
- **Artist:** [Hồ Sỹ Tính](https://www.facebook.com/than.cham.7737)

---

> 🎓 Đây là sản phẩm được thực hiện trong môi trường [CLB Nhà Sáng Tạo Game PTIT](https://www.facebook.com/gamecreatorsclub), nơi các thành viên cùng học hỏi, thử nghiệm ý tưởng và phát triển kỹ năng làm game.
