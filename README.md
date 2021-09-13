# 항만 안전운항 솔루션
- 5G와 수상드론을 통한 울산항 안전운항 관리 솔루션 및 시뮬레이션

## Project Introduction
- 해양사고에 소형 선박의 운행자들의 운항 실력 및 안전 의식이 많은 영향을 미치고 있다. 이로 인한 선박 사고를 방지하기 위해 도선사를 위한 안전교육 및 운항교육 도입과 웹 기반의 VTS 시스템을 통한 VTS 이중화로 항만관계자 및 도선사들의 사고로 인한 피해를 줄인다.


## Project Overview
### 전체 구조도
<img src="https://github.com/yjyjy131/safeHarbor/blob/master/project_asset/img/structure.jpg" width="800px">

### 1. 가상현실 시뮬레이션
<img src="https://github.com/yjyjy131/safeHarbor/blob/master/project_asset/img/simul1.jpg" width="300px"> <img src="https://github.com/yjyjy131/safeHarbor/blob/master/project_asset/img/simul2.jpg" width="300px">  

### 2. 수상드론 및 조타기
<img src="https://github.com/yjyjy131/safeHarbor/blob/master/project_asset/img/ship1.jpg" width="230px">  <img src="https://github.com/yjyjy131/safeHarbor/blob/master/project_asset/img/ship2.jpg" width="190px"> <img src="https://github.com/yjyjy131/safeHarbor/blob/master/project_asset/img/controller.jpg" width="200px">

### 3. 면적 기반 충돌 감지 시스템
<img src="https://github.com/yjyjy131/safeHarbor/blob/master/project_asset/img/vts1.jpg" width="300px"> <img src="https://github.com/yjyjy131/safeHarbor/blob/master/project_asset/img/vts2.jpg" width="300px">  

## Project Development Process
<img src="https://github.com/yjyjy131/safeHarbor/blob/master/project_asset/img/schedule.png" width="500px">  


## Project Main Function
|**기능**|**설명**|
|---|-----------------------------------------------------------------|
| 가상현실 시뮬레이션 | VR glass를 이용해 가상현실로 안전운항 연습 가능 |
| 라즈베리파이 서버 | 안드로이드, 웹, 수상드론간의 통신을 5G네트워크로 연결해주는 서버 역할 |
| 안드로이드 | 수상드론을 조종하고, 서버에 위치 및 영상 정보 전송 |
| 실시간 관제 시스템 | 수상드론 스트리밍 화면 및 각종 정보를 확인하고, 현재 실제 선박 위치 및 정보를 실시간 전달|
| 수상드론 | 시뮬레이션 교육을 마친 후 2차 운항 교육에 사용 / 실시간 관제 시스템에서 선박의 위,경도 정보를 전송|
| 조타기 | 가상현실 시뮬레이션과 수상드론의 조종에 쓰이는 컨트롤러 |


## Project Documents
:link:[safeHarbor/project_asset/docu](https://github.com/yjyjy131/safeHarbor/tree/master/project_asset/docu)
1. 프로젝트 최종 결과 보고서
2. 정보처리학회지 논문 
3. 특허 출원서 


## Team Members
|**이름**|**담당**|**Github**|
|--------|--------|-------------------------------------------------------|
|김정민| 프로젝트 멘토 |-|
|김성연|백엔드, 시뮬레이션|:link:[SibaDoge1](https://github.com/SibaDoge1)| 
|김연진|프론트엔드, 백엔드|:link:[yjyjy131](https://github.com/yjyjy131)|
|김정수|하드웨어, 시뮬레이션|:link:|
|황준호|시뮬레이션, 프론트엔드|:link:|
