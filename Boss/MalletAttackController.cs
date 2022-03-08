using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AWMalletAttackController : MonoBehaviour
    {
        #region BossRequiredComponents
        
        public Transform _posCenter;
        public int rotationLimitX;
        public int multipleAttackHitLimit;
        
        [SerializeField] Transform _posLeftLong;
        [SerializeField] Transform _posRightLong;
        [SerializeField] private Transform transformModel;
        [SerializeField] private float bossSpeed = 5f;
        [SerializeField] Vector3[] _weaponHitBoxSize;
        [SerializeField] Vector3[] _weaponHitBoxOffset;
        [SerializeField] private int[] _movimentAnimation;
        [SerializeField] private float[] bossSpeedToReachChain;
        [SerializeField] AWColliderBox _frontCollider;
        
        private Vector3 _targetPosition;
        private float bossMoveSpeed;
        private float bossMoveSpeedBase;
        Vector3 transTarget;
        List<Vector3> _pListTemp;
        
        #endregion
        
        #region BossControllers
        
        [SerializeField] private int _indexSizeChain;
        
        float _actionIndex;
        private bool _resetSpeed;
        private bool _targetOnLeft;
        bool _alreadyChosedDestination;
        private bool _canAddWeaponSize;
        private bool _canCheckTarget;
        private bool _controlAnimWalk;
        private int _indexAttacksOne = 2;
        private bool _setRockTrue;
        private int _roundRocks;
        private bool _resetRocks;
        private bool _longAttackDone;
        private List<float> canceledValues;
        int _randomIndexRock;
        int _movimentAnimationBase;
        bool _stopAttack;
        #endregion
        
        #region BossRequiredScripts
        
        public AWMalletStateManager aWMalletStateManager;
        
        [SerializeField] Enemy.Hitbox.AWEnemyHitbox weaponEnemyHitbox;
        [SerializeField] private Vector3[] hitBoxAttackRotationSize;
        [SerializeField] private Vector3[] hitBoxAttackMultipleHitSize;
        [SerializeField] private Vector3 hitBoxAttackLongHit;
        [SerializeField] Systems.AWBossLife awBosslifeManager;
        
        AWMalletBossIdle _awMalletBossIdle;
        Enemy.Hitbox.AWEnemyHitbox enemyHitbox;
        AWMultipleAttack multipleAttack;
        AWMultipleFollow _multipleFollow;
        AWMalletAnimation malletAnimation;
        
        #endregion
        
        #region ObstacleManager
        
        [SerializeField] private List<GameObject> _rocksObstacle = new List<GameObject>();
        [SerializeField] private List<GameObject> _rocksObstacleTrigger = new List<GameObject>();
        
        private List<AWRocksObstacle> _rockObstacleScript = new List<AWRocksObstacle>();
        
        public void ShowRockObstacleTemplates(int index)
        {
            List<AWRockObstaclePreset> tempList = _listOfPresets[index].RockObstaclePresets;
            int count = tempList.Count;
            for (int i = 0; i < count; i++)
            {
                tempList[i].RockObstacleTrigger.SetActive(tempList[i].Value);
            }
        }
        
        [SerializeField] private List<AWRockObstaclePresetList> _listOfPresets = new List<AWRockObstaclePresetList>();

        private void ClearPresets()
        {
            int count = _rocksObstacle.Count;
            for (int i = 0; i < count; i++)
            {
                if (_rocksObstacle[i].activeSelf)
                    _rocksObstacle[i].GetComponent<AWRocksObstacle>().FinishHit();
                _rocksObstacle[i].SetActive(false);
            }
            _roundRocks++;
            if (_roundRocks <= _indexSizeChain)
            {
                SetRockObstacle();
            }
            else
            {
                _setRockTrue = false;
                _roundRocks = 0;
            }

        }
       
        private bool HaveShocksActive()
        {
            int count = _rocksObstacle.Count;
            for (int i = 0; i < count; i++)
            {
                if (_rocksObstacle[i].activeInHierarchy) return true;
            }

            return false;
        }

        private int _lastPresetsCount = 0;

        private void SetPresets()
        {
            if (_lastPresetsCount >= _listOfPresets.Count)
            {
                _lastPresetsCount = _listOfPresets.Count;
                return;
            }

            _listOfPresets[_listOfPresets.Count - 1] = new AWRockObstaclePresetList(_rocksObstacle, _rocksObstacle);
            _lastPresetsCount = _listOfPresets.Count;
        }

        public bool HasActiveRockObstacle()
        {
            return _setRockTrue;
        }
        public void RockObstacleManager()
        {
            if (!_setRockTrue && !_resetRocks)
            {
                _setRockTrue = true;
                _roundRocks = 0;
                SetRockObstacle();
            }
        }
        private void SetRockObstacle()
        {
            _randomIndexRock = UnityEngine.Random.Range(0, _listOfPresets.Count);

            List<AWRockObstaclePreset> tempList = _listOfPresets[_randomIndexRock].RockObstaclePresets;
            int count = tempList.Count;

            StartCoroutine(ActiveRockObstacle(_randomIndexRock));
        }

        private IEnumerator ActiveRockObstacle(int index)
        {
            List<AWRockObstaclePreset> tempList = _listOfPresets[index].RockObstaclePresets;
            int count = tempList.Count;
            for (int i = 0; i < count; i++)
            {
                tempList[i].RockObstacle.SetActive(tempList[i].Value);
                tempList[i].RockObstacle.GetComponent<ArchWorks.Obstacles.AWRocksObstacle>().DoAnticipateButton();
                tempList[i].RockObstacleTrigger.SetActive(false);
            }

            while (!_finishedHit())
            {
                yield return null;
            }

            for (int i = 0; i < _rockObstacleScript.Count; i++)
            {
                if (_rockObstacleScript[i].FinishedHit())
                {
                    _rockObstacleScript[i].SetFinishHit(false);
                }
            }
            ClearPresets();
        }

        private bool _finishedHit()
        {
            for (int i = 0; i < _rockObstacleScript.Count; i++)
            {
                if (_rockObstacleScript[i].gameObject.activeInHierarchy && !_rockObstacleScript[i].FinishedHit())
                {
                    return false;
                }
            }

            return true;
        }

        private void ResetRockObstacle()
        {
            _setRockTrue = false;
            _roundRocks = _indexSizeChain;
            int count = _rocksObstacle.Count;
            for (int i = 0; i < count; i++)
            {
                if (_rocksObstacle[i].activeSelf)
                {
                    _rocksObstacle[i].GetComponent<ArchWorks.Obstacles.AWRocksObstacle>().FinishHit();
                    _rockObstacleScript[i].SetFinishHit(false);
                }
                _rocksObstacle[i].SetActive(false);
            }
        }
        #endregion
        
        void Awake()
        {
            multipleAttack = GetComponent<AWMultipleAttack>();
            malletAnimation = GetComponent<AWMalletAnimation>();
            enemyHitbox = GetComponent<Enemy.Hitbox.AWEnemyHitbox>();
            _awMalletBossIdle = GetComponent<AWMalletBossIdle>();
            _multipleFollow = GetComponent<AWMultipleFollow>();
            _pListTemp = new List<Vector3>();
            _rockObstacleScript = new List<AWRocksObstacle>();
            for (int i = 0; i < _rocksObstacle.Count; i++)
            {
                _rockObstacleScript.Add(_rocksObstacle[i].GetComponent<AWRocksObstacle>());
            }
            AWGameplayState.Instance.OnPlayerDeath += StopExecuting;
        }

        void Update()
        {
            switch (aWMalletStateManager.GetCurrentProcessState())
            {
                case AWMalletStateManager.ProcessState.FinishingAttack:
                    _controlAnimWalk = false;
                    _alreadyChosedDestination = false;
                    break;
                default:
                    break;
            }
            CheckIfReachedPosition();
        }
        void FixedUpdate()
        {
            DoActualState();
        }

        private void CheckIfReachedPosition()
        {
            if (!_awMalletBossIdle.getReachedPosition() && _canCheckTarget)
            {
                if (!_controlAnimWalk)
                {
                    RotateModel(1, transform.position.z, _targetPosition.z);
                    malletAnimation.SetAnimations("Walk");
                    _controlAnimWalk = true;
                }
                if (transform.position == _targetPosition)
                {
                    SetActionIndex(1);
                    _awMalletBossIdle.setBool(0, true);
                    _canCheckTarget = false;
                }
            }
        }
        
        public void StopExecuting()
        {
            malletAnimation.SetAnimations("Idle");
            aWMalletStateManager.SetNextState(AWMalletStateManager.Command.PlayerDead);
        }
        
        private void DoActualState()
        {
            switch (_actionIndex)
            {
                case 1:
                    //parar o boss
                    if (bossMoveSpeed != 0)
                        bossMoveSpeed = 0;
                    if (_movimentAnimationBase != 0)
                        _movimentAnimationBase = 0;
                    if (_resetSpeed)
                        _resetSpeed = false;
                    break;
                case 2:
                    //se movendo para um ponto de ataque
                    MoveToMainPosition(this.transform, _targetPosition);
                    break;
                case 3:
                    //se movendo durante o ataque um
                    MoveFunction();
                    break;
            }
        }
        
        public void SetActionIndex(int indexAction)
        {
            _actionIndex = indexAction;
        }

        public void SetInvulnerable(bool setInvincible)
        {
            awBosslifeManager.SetInvulnerable(setInvincible);
        }
        
        public void ChangeIndexSizeChain(int newNumber)
        {
            _canAddWeaponSize = true;
            if (aWMalletStateManager.GetProcessStateName() != "FinishingAttack")
            {
                StartCoroutine(IncreaseWeaponSize(newNumber));
            }
            else
            {
                _indexSizeChain = newNumber;
                _canAddWeaponSize = false;
                _awMalletBossIdle.SetIncreasedValue(1);
                ResetRockObstacle();
            }
        }
        
        private IEnumerator IncreaseWeaponSize(int indexNumber)
        {
            while (aWMalletStateManager.GetProcessStateName() != "FinishingAttack")
            {
                yield return null;
            }
            _indexSizeChain = indexNumber;
            _canAddWeaponSize = false;
            _awMalletBossIdle.SetIncreasedValue(1);
            ResetRockObstacle();

        }

        public int indexSizeChainBase()
        {
            return _indexSizeChain;
        }

        public void callRotation()
        {
            float dist = Vector3.Distance(transform.position, _posCenter.position);
            if (dist > 1f)
                RotateModel(0, transform.position.z, _posCenter.position.z);

            if (_longAttackDone)
            {
                _multipleFollow.FinishAttackAnimation();
                _longAttackDone = false;
            }
        }

        public void SetHitBox(int i, bool activateWeapon)
        {
            switch (i)
            {
                case 4:
                    //Hit box do ataque girando
                    enemyHitbox.enabled = activateWeapon;
                    enemyHitbox.ChangeHitBoxSize(hitBoxAttackRotationSize[_indexSizeChain]);
                    break;
                case 1:
                    //Hit box do Ataque Multiplo.
                    enemyHitbox.enabled = activateWeapon;
                    enemyHitbox.ChangeHitBoxSize(hitBoxAttackMultipleHitSize[_indexSizeChain]);
                    break;
                case 3:
                    //Hitbox da arma sozinha
                    weaponEnemyHitbox.enabled = activateWeapon;
                    weaponEnemyHitbox.ChangeHitBoxSize(_weaponHitBoxSize[_indexSizeChain]);
                    if (_targetOnLeft)
                        weaponEnemyHitbox.ChangeHitBoxOffset(_weaponHitBoxOffset[_indexSizeChain]);
                    else weaponEnemyHitbox.ChangeHitBoxOffset(new Vector3(-_weaponHitBoxOffset[_indexSizeChain].x, _weaponHitBoxOffset[_indexSizeChain].y, -_weaponHitBoxOffset[_indexSizeChain].z));
                    break;
                case 2:
                    //Hitbox da arma e do boss durante o Ataque Longo
                    enemyHitbox.enabled = activateWeapon;
                    enemyHitbox.ChangeHitBoxSize(hitBoxAttackLongHit);
                    break;
            }
        }
        
        public void RotateModel(int indexRotateModel, float transformPosition, float targetPositionZ)
        {
            switch (indexRotateModel)
            {
                case 0:
                    if (transformModel.localEulerAngles.y >= 180)
                        transformModel.localEulerAngles = Vector3.zero;
                    else
                    {
                        transformModel.localEulerAngles = new Vector3(0, 180, 0);
                    }
                    break;
                case 1:
                    float distBoth = transformPosition - targetPositionZ;

                    if (distBoth > 0)
                        transformModel.localEulerAngles = new Vector3(0, 180, 0);
                    else if (distBoth < 0)
                        transformModel.localEulerAngles = Vector3.zero;
                    break;
            }
        }

        public void RotateToBehind()
        {
            float yAxis = 180 * transformModel.forward.z;
            float yAxisResult = transformModel.eulerAngles.y;
            yAxisResult += yAxis;

            transformModel.eulerAngles = new Vector3(0, yAxisResult, 0);
        }

        public void GetDestinationToAttack(string baseAttackIndex)
        {
            switch (baseAttackIndex)
            {
                case "LongHitAttack":
                    GetPositionToLongHitHammer();
                    break;
                case "RotateHitAttack":
                    GetPositionToRotateHitAttack();
                    break;
                case "MultipleHammerHitAttack":
                    GetPositionToMultipleHitAttack();
                    break;
            }
        }

        public void GetPositionToLongHitHammer()
        {
            float dist = Vector3.Distance(transform.position, _posLeftLong.position);
            float dist2 = Vector3.Distance(transform.position, _posRightLong.position);

            int randomNumber = Random.Range(0, 2);
            if (!_alreadyChosedDestination)
            {
                if (dist < 3)
                {
                    _targetPosition = new Vector3(transform.position.x, transform.position.y, _posLeftLong.position.z);
                    _targetOnLeft = true;
                }
                else if (dist2 < 3)
                {
                    _targetPosition = new Vector3(transform.position.x, transform.position.y, _posRightLong.position.z);
                    _targetOnLeft = false;
                }
                else
                {
                    if (randomNumber == 0)
                    {
                        _targetPosition = new Vector3(transform.position.x, transform.position.y, _posLeftLong.position.z);
                        _targetOnLeft = true;
                    }
                    else
                    {
                        _targetPosition = new Vector3(transform.position.x, transform.position.y, _posRightLong.position.z);
                        _targetOnLeft = false;
                    }
                }
                _alreadyChosedDestination = true;
            }
            _canCheckTarget = true;
            _actionIndex = 2;
        }

        public void GetPositionToRotateHitAttack()
        {
            _targetPosition = _posCenter.position;

            _canCheckTarget = true;
            _actionIndex = 2;
        }

        public void GetPositionToMultipleHitAttack()
        {
            _targetPosition = _posCenter.position;
            _canCheckTarget = true;
            _actionIndex = 2;
        }

        public void MoveToMainPosition(Transform moveTarget, Vector3 target)
        {
            bossMoveSpeed = bossSpeed;
            moveTarget.localPosition = Vector3.MoveTowards(moveTarget.localPosition, target, bossMoveSpeed * Time.deltaTime);
        }

        public void MoveFunction()
        {
            if (_movimentAnimationBase >= _movimentAnimation[_indexSizeChain])
            {
                SetActionIndex(0);
                return;
            }

            if (!_resetSpeed)
            {
                bossMoveSpeedBase = bossSpeedToReachChain[_indexSizeChain];
                if (!_targetOnLeft)
                {
                    bossMoveSpeed = 0;
                }
                else
                {
                    bossMoveSpeed = 0;
                }
                _resetSpeed = true;
            }

            bossMoveSpeed += bossMoveSpeedBase * Time.deltaTime;

            transform.Translate(bossMoveSpeed * transformModel.forward);
            _movimentAnimationBase += 1;
        }

        public void GetCharacterPosition()
        {
            AWCharacterBehind characterBehind = GetComponent<AWCharacterBehind>();
            if (characterBehind == null) return;

            Collider[] collidedObjects = Physics.OverlapBox(transform.position + _frontCollider.ForwardOf(transformModel), _frontCollider.Size / 2, Quaternion.identity, _frontCollider.Layermask);

            if (collidedObjects.Length != 0)
            {
                RotateToBehind();
                return;
            }

            characterBehind.VerifyIfPlayerIsBehind();
        }

        public void FinishAttackOne()
        {
            SetHitBox(2, false);
            _indexAttacksOne--;
            if (_indexAttacksOne < _indexSizeChain)
            {
                multipleAttack.FinishHitAnimation();
                malletAnimation.SetAnimations("Attack1Follow");
                _longAttackDone = true;
                _indexAttacksOne = 2;
            }
            _actionIndex = 1;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = _frontCollider.DrawColor;
            Gizmos.DrawWireCube(transform.position + _frontCollider.ForwardOf(transformModel), _frontCollider.Size);
        }
    }
