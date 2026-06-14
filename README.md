# BlueM.TENConverter

A command-line utility for converting old binary TeeChart TEN files to the new JSON format

## Usage
```cmd
TENConverter.exe [-o outputdirectory] paths
    -o: optional output directory. If not provided, converted files are saved to the same directory as the original
    paths: one or more space-separated paths to TEN files to convert. If path is a directory, all TEN files found in this directory including subdirectories will be converted.
Converted files will have the same filename as the original but with file extension .json.ten
```

## Examples

Convert a single file:
```cmd
TenConverter.exe C:\Charts\chart1.ten
```
Saves the converted file to `C:\Charts\chart1.json.ten`

---

Convert a single file and save to a different directory:
```cmd
TenConverter.exe -o C:\ConvertedCharts C:\Charts\chart1.ten
```
Saves the converted file to `C:\ConvertedCharts\chart1.json.ten`

---

Convert multiple files:
```cmd
TenConverter.exe C:\Charts\chart1.ten C:\Charts\chart2.ten
```
Saves the converted files to `C:\Charts\chart1.json.ten` and `C:\Charts\chart2.json.ten`

---

Convert multiple files and save to a different directory:
```cmd
TenConverter.exe -o C:\ConvertedCharts C:\Charts\chart1.ten C:\Charts\chart2.ten
```
Saves the converted files to `C:\ConvertedCharts`

---

Convert all TEN files in a directory and its subdirectories:
```cmd
TenConverter.exe C:\Charts
```
Saves the converted files to the same directories as the originals

---
Convert all TEN files in a directory and save converted files to a different directory:
```cmd
TenConverter.exe -o C:\ConvertedCharts C:\Charts
```
Saves all converted files to `C:\ConvertedCharts`