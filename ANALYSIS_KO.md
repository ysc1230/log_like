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

### 간접 영향(없음/분리됨)
- `PlayerStats.AttackCooldown`은 `AutoAttacker`(기본 발사체 자동공격) 전용이며, `WeaponBase` 계열 3종 무기 주기에는 연결되지 않는다.
- `AddAttackRateMultiplier()`를 올려도 FishBone/CatClaw/TunaCanBomb의 주기는 변하지 않는다.

## 3) 정리
- 현재 구조에서 무기 3종의 공격 주기는 **무기 내부 상수 + 무기 레벨(4+) + 타임스케일(일시정지)** 에 의해 결정된다.
- 플레이어 스탯의 공격속도 계열 값은 별도 시스템(`AutoAttacker`)에만 적용된다.
