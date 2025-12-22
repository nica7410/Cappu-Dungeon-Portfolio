# Cappu-Dungeon-Portfolio

카푸 던전 스킬 소개<br>
https://www.notion.so/262f15e077a98056a91be2ebcf1164b2?source=copy_link

트레일러 영상<br>
https://www.youtube.com/watch?v=d0scGXJc0s4

스킬은 발동 방식과 형태에 따라 구분됩니다.

다운 : 키 다운 발동<br>
업 : 키 업 발동<br>
※ 기획 변경에 따라 현재는 다운 기능만 사용 중. 업 스킬도 다운으로 발동<br>

일반 : 캐릭터 주위에 발동<br>
방향 : 캐릭터 위치에서 마우스 방향으로 스킬 발동<br>
범위 : 마우스 위치에서 발동<br>

--------

카푸 던전 포트폴리오 - 소스 코드

몬스터 패턴 시스템<br>
패턴 로직은 코드로 정의하되, 노드 기반 커스텀 에디터에서 조합해 동시·순차 발동 흐름을 구성하고 ScriptableObject로 저장할 수 있도록 구축했습니다.

Flame Spirit 몬스터 패턴 노드 <br>
<img width="1260" height="1375" alt="image" src="https://github.com/user-attachments/assets/cab59ce9-79d0-4742-97dc-e364a21b8eef" />

Flame Spirit 몬스터 시전 영상<br>
<video src="https://github.com/user-attachments/assets/23f4a9f7-eb8b-40f5-b625-e35c80688e04"
       autoplay loop muted playsinline
       style="max-width:100%; height:auto; border-radius:12px;"></video>
       
어드레서블 기반 제네릭 오브젝트 풀<br>
AddressableObjectPool<T>로 비동기 로드→인스턴스화→큐 재사용 흐름을 구성하고, EnemyAdrObjPool에서 적 스폰/반납, 보스 시작 처리 등 게임 로직에 맞게 확장했습니다. 

유니크 스킬 아키텍처<br>
IUniqueSkillComponent 인터페이스와 UniqueSkillComponentBase를 통해 스킬 실행 타입, 선택 액션(onPointerDown/Up 등), 대기 동작(Wait), 이펙트 레벨 관리, 슬롯 UI 연동을 모듈화했습니다. 

예시 스킬 구현<br>
FlameBreath(방향 지정 스킬)과 Blizzard(범위 지정 스킬)는 이펙트/사운드 로드, RangeDamage/Bullet 액션 조합, 속성 로직을 포함합니다. 

FlameBreath 시전 영상<br>
<video src="https://github.com/user-attachments/assets/9363f5a9-5fed-4f4b-b994-453972e69e14"
       autoplay loop muted playsinline
       style="max-width:100%; height:auto; border-radius:12px;"></video>

Blizzard 시전 영상<br>
<video src="https://github.com/user-attachments/assets/42f0b5e7-1907-4b29-8af6-830e3868fe2c"
       autoplay loop muted playsinline
       style="max-width:100%; height:auto; border-radius:12px;"></video>





