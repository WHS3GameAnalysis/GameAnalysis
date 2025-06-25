## 빌드 방법

먼저 비주얼 스튜디오에서 본 리포지토리를 복제하거나 다운받아 LethalHack.sln 파일을 열어줍니다.

그 후, 비주얼 스튜디오 상단의 '빌드' - '솔루션 빌드'를 눌러 빌드를 진행합니다. (Ctrl + Shift + B)

빌드가 성공적으로 완료되었다면, `LethalHack/bin/Debug` 폴더 내에 LethalHack.dll 파일이 생성됩니다.

## 빌드 오류 관련
만약 참조 오류가 발생할 경우에는 솔루션 탐색기에서 '참조' 우클릭 > '참조 추가'를 눌러 리썰 컴퍼니 게임경로에 있는 Managed 안의 dll들을 추가해주면 됩니다.

## 통합된 기능들
0. SuperJump/GodMode/InfiStamina/HPDisplay
1. Enemy/Novisor
2. FastClimb
3. ESP(Item,Enemy)
4. Teleport/DamageHack/Minimap
5. InputSeed
6. Noclip(예정)
7. AntiKick(예정)
8. Freecam(예정)

## 주의 사항
리썰 컴퍼니 실행하자마자 inject 하면 esp가 작동을 안해서 함선에 들어가서 inject 해야합니다.

## 안고친거
1. 토글불가 : Minimap/Novisor/NightVision
2. ESP 기능 Enemy/Item 객체 진입점 수정
