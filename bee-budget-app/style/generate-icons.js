// 执行命令：node generate-icons.js

const fs = require('fs');
const path = require('path');

// 1. 配置路径 (请根据你的实际路径调整)
const scssPath = path.join(__dirname, 'iconfont.scss');
const outputPath = path.join(__dirname, 'icon-list.js');

// 2. 检查文件是否存在
if (!fs.existsSync(scssPath)) {
    console.error(`❌ 找不到文件: ${scssPath}`);
    process.exit(1);
}

// 3. 读取文件内容
const content = fs.readFileSync(scssPath, 'utf-8');

// 4. 正则匹配
// 匹配 .custom-icon-xxx:before 中的 xxx
const regex = /\.custom-icon-([^\s:]+):before/g;
let match;
const iconList = [];

// 5. 提取数据
while ((match = regex.exec(content)) !== null) {
    // 关键修正：使用 match[1] 获取括号内捕获的类名
    iconList.push(match[1]);
}

// 6. 生成 JS 文件内容
const fileContent = `// 此文件由脚本自动生成，请勿手动修改
export default ${JSON.stringify(iconList, null, 2)};
`;

// 7. 写入文件
const dir = path.dirname(outputPath);
if (!fs.existsSync(dir)) {
    fs.mkdirSync(dir, { recursive: true });
}

fs.writeFileSync(outputPath, fileContent);

console.log(`✅ 成功提取 ${iconList.length} 个图标！`);
console.log(`📄 文件已保存至: ${outputPath}`);