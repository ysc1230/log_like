# Unity 2D Android 뱀서류 최소 프로토타입

## 1) 씬/오브젝트 구성 제안 (MainScene)
- `Main Camera` (Orthographic, Portrait)
- `GameRoot`
  - `GameBootstrap`
  - `EnemySpawner`
  - `EnemyRegistry`
  - `LevelSystem`
  - `ProjectilePool`
- `Player`
  - `SpriteRenderer`(기본 원형/사각)
  - `Rigidbody2D`(Dynamic), `CircleCollider2D`
  - `PlayerStats`, `PlayerMover`, `AutoAttacker`, `LevelSystemLink`
  - `FirePoint` (자식)
- `Canvas` (Screen Space - Overlay)
  - `HUD`
    - HP Slider
    - EXP Slider
    - Level Text (TMP)
    - Time Text (TMP)
  - `JoystickArea` (좌하단 투명 패널)
  - `LevelUpPanel` (버튼 3개)
  - `GameOverPanel` (재시작 버튼)
- `Prefabs`
  - `Enemy` (SpriteRenderer, Collider2D, EnemyController)
  - `Projectile` (SpriteRenderer, Trigger Collider2D, Projectile)
  - `ExpOrb` (SpriteRenderer, Trigger Collider2D, ExpOrb)

## 2) 스크립트 목록
- Core
  - `GameEvents.cs`
  - `GameBootstrap.cs`
- Input
  - `TouchJoystickInput.cs`
- Player
  - `PlayerStats.cs`
  - `PlayerMover.cs`
- Enemy
  - `EnemyController.cs`
  - `EnemySpawner.cs`
  - `EnemyRegistry.cs`
- Combat
  - `AutoAttacker.cs`
  - `Projectile.cs`
  - `ProjectilePool.cs`
- Progression
  - `ExpOrb.cs`
  - `LevelSystem.cs`
  - `LevelSystemLink.cs`
- UI
  - `HudController.cs`
  - `LevelUpPanel.cs`
  - `GameOverPanel.cs`

## 3) Unity 에디터 작업 순서
1. Unity Hub에서 2D(Core) 프로젝트 생성.
2. `MainScene` 생성 후 Build Settings에 등록.
3. Portrait 고정: `Project Settings > Player > Resolution and Presentation > Default Orientation = Portrait`.
4. 위 오브젝트 트리대로 생성.
5. `Enemy`, `Projectile`, `ExpOrb` 프리팹 생성.
6. 각 스크립트를 `Assets/Scripts/...`에 생성 후 컴파일.
7. 인스펙터 연결:
   - `PlayerMover`: PlayerStats, TouchJoystickInput, MainCamera 연결
   - `AutoAttacker`: PlayerStats, ProjectilePool, FirePoint, EnemyRegistry 연결
   - `EnemySpawner`: Enemy Prefab, Player Transform, MainCamera, EnemyRegistry, LevelSystem 연결
   - `EnemyController`: ExpOrb Prefab 연결
   - `LevelSystem`: PlayerStats 연결
   - `LevelSystemLink`: LevelSystem 연결
   - `LevelUpPanel`: Root(패널), LevelSystem 연결 + 버튼 OnClick 연결
   - `GameOverPanel`: Root(패널), Restart 버튼 OnClick 연결
   - `HudController`: HP/EXP Slider, TMP 텍스트 연결
   - `TouchJoystickInput`: JoystickArea RectTransform 연결
   - `ProjectilePool`: Projectile Prefab 연결
8. 물리 레이어 세팅:
   - Projectile는 Enemy와만 Trigger 충돌
   - Player/Enemy는 충돌 활성
9. 플레이 테스트로 전투 루프 확인.

## 4) Android APK 빌드 전 체크리스트
- Android Build Support 설치(IL2CPP + SDK/NDK/OpenJDK).
- `Build Settings > Android > Switch Platform`.
- `Player Settings`:
  - Package Name 설정 (예: `com.yourname.survivor2d`)
  - Minimum API Level (권장 Android 8.0+)
  - Scripting Backend = IL2CPP
  - Target Architectures = ARM64 체크
  - Orientation = Portrait
- `MainScene`이 Scenes In Build에 포함됐는지 확인.
- Development Build 체크 후 첫 설치 테스트 권장.

## 5) 안드로이드 실기기 실행(짧게)
1. 폰에서 개발자 옵션 + USB 디버깅 ON.
2. USB 연결 후 `Build And Run`.
3. 설치된 APK 실행 후 조이스틱 이동/자동공격/레벨업/게임오버 확인.

## 6) 다음 단계 추천 기능 3개
1. 무기 시스템 확장(관통/범위/다중 발사).
2. 적 타입 다양화(원거리/돌진/탱커).
3. 영구 성장(재화 + 메타 업그레이드 저장).
