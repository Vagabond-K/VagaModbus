# VagaModbus Analyzer
<a href="https://apps.microsoft.com/store/detail/9pg8qtrlp62x?cid=github&mode=direct"><img src="https://learn.microsoft.com/en-us/windows/apps/images/new-badge-light.png" alt="Download from the Microsoft Store" width='320' /></a>

본 응용프로그램은 Modbus Slave 디바이스와 통신하는 기능을 제공합니다.  

Modbus RTU, Modbus TCP, Modbus ASCII 등의 프로토콜 버전을 지원하며, 통신 채널로 TCP 클라이언트, TCP 서버, UDP 소켓, 시리얼 포트를 이용할 수 있습니다. 단, TCP 서버 채널은 UWP 앱의 정책으로 인해 루프백 연결은 허용되지 않습니다.  

레지스터 데이터들은 마우스로 드래그하여 여러 개의 바이트들을 선택 가능하고, 선택된 바이트들을 정수 형식, 단정밀도 부동 소수점 형식, 배정밀도 부동 소수점 형식의 값으로 읽을 수 있습니다.  

그리고 Endian을 고려하여 Holding Register에 정수와 소수를 쓸 수 있으며 Coil에는 비트 쓰기가 가능합니다.  

또한 Slave Address를 모를 경우, 범위를 설정하여 순차적으로 읽기 시도를 해볼 수 있습니다.  

마지막으로, 통신 중에 발생한 요청/응답 메시지를 기록하여 조회할 수 있는 로깅 기능을 제공하고 있습니다.  

## 업데이트 내용 및 사용법
- [v1.0](https://blog.naver.com/vagabond-k/222488169575): 통신 채널 관리 및 데이터 읽기, 로깅 기능 제공
- [v1.1](https://blog.naver.com/vagabond-k/222619234580): Holding Register 및 Coil 쓰기 기능 제공
- [v1.2](https://blog.naver.com/vagabond-k/223948738205): Slave Address 찾기 기능 제공
