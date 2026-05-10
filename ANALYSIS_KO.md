# 전투 시뮬레이션 분석 (캐릭터/적/무기 기준)

## 1) 무기 공격 주기(핵심)

### 공통 동작 (`WeaponBase`)
- 모든 무기는 `WeaponBase.Update()`에서 `cooldownTimer -= Time.deltaTime`로 쿨다운을 소모한다.
- `cooldownTimer <= 0`이 되면 `Attack()` 실행 후 `ResetCooldown()`으로 다음 주기를 설정한다.
- 무기를 처음 획득할 때(`SetLevel`에서 `level==0 -> newLevel>0`) 초기 쿨다운은 `0.0~0.5초` 랜덤 지연이 들어간다.

### 무기별 실제 주기
- **FishBoneWeapon**
  - 기본 주기: `2.4초`
  - 레벨 4 이상: `2.4 * 0.7 = 1.68초`
- **CatClawWeapon**
  - 기본 주기: `4.0초`
  - 레벨 4 이상: `4.0 * 0.7 = 2.8초`
- **TunaCanBombWeapon**
  - 기본 주기: `7.0초`
  - 레벨 4 이상: `7.0 * 0.7 = 4.9초`

## 2) 무기 공격 주기에 영향 주는 다른 위치

### 직접 영향(있음)
1. `WeaponBase.SetLevel()`
   - 첫 획득 시 초기 랜덤 지연(0~0.5초)으로 첫 발사 타이밍이 바뀜.
2. 각 무기의 `ResetCooldown()`
   - 레벨 4 이상 조건으로 주기 30% 단축.
3. `LevelSystem` + `LevelUpPanel`
   - 레벨업 시 `Time.timeScale = 0f`로 게임이 일시정지되어 무기 쿨다운 감소도 멈춤.
   - 선택 완료 후 `Time.timeScale = 1f`로 재개.
4. `WeaponManager.AddOrUpgradeWeapon()`
   - 강화 시 `SetLevel(status.Level)` 호출로 이후 `ResetCooldown()`에서 레벨 반영 주기로 순환.

### 간접 영향(현재 구현 기준 연결됨)
- `PlayerStats.AttackCooldown` 변경은 `GameEvents.OnStatsChanged`를 통해 `WeaponManager`로 전달된다.
- `WeaponManager`는 `baseAttackCooldown / attackCooldown` 비율로 무기 공속 배율을 계산해 활성 무기 전체에 적용한다.
- 따라서 `AddAttackRateMultiplier()`로 공격속도가 빨라지면 FishBone/CatClaw/TunaCanBomb의 실제 주기도 함께 빨라진다.

## 3) 정리
- 현재 구조에서 무기 3종의 공격 주기는 **무기 내부 상수 + 무기 레벨(4+) + 타임스케일(일시정지) + 플레이어 공격속도 배율**로 결정된다.
- 플레이어 공격속도 변경은 `WeaponManager`가 활성 무기 전체에 공통 배율로 반영한다.
