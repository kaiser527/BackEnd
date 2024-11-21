/Chạy dự án/
npm run dev

/Tải toàn bộ package trước khi chạy dự án/
npm i

/Sửa tên database bên trong file .env và sữa bên trong file config.json y hệt trong file .env/

/Sau khi chạy dự án vào terminal paste 2 câu lệnh sau/

npx sequelize-cli db:migrate
npx sequelize-cli db:seed:all
