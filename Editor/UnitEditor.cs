#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Unit))]
public class UnitEditor : Editor
{
    private SerializedProperty _config;
    private SerializedProperty _icon;
    private SerializedProperty _playerId;
    private SerializedProperty _selectableByLocalPlayer;
    private SerializedProperty _animator;
    private SerializedProperty _visualRoot;
    private SerializedProperty _idleAnimationStateName;
    private SerializedProperty _runAnimationStateName;
    private SerializedProperty _buildAnimationStateName;
    private SerializedProperty _attackAnimationStateName;
    private SerializedProperty _warriorAttackAnimationStateNameA;
    private SerializedProperty _warriorAttackAnimationStateNameB;
    private SerializedProperty _spearmanAttackRightAnimationStateName;
    private SerializedProperty _spearmanAttackUpAnimationStateName;
    private SerializedProperty _spearmanAttackDownAnimationStateName;
    private SerializedProperty _spearmanAttackUpRightAnimationStateName;
    private SerializedProperty _spearmanAttackDownRightAnimationStateName;
    private SerializedProperty _warriorAttackDuration;
    private SerializedProperty _spearmanAttackDuration;
    private SerializedProperty _archerAttackDuration;
    private SerializedProperty _defaultAttackDuration;
    private SerializedProperty _animationCrossFadeDuration;
    private SerializedProperty _animationLayerIndex;
    private SerializedProperty _arrowPrefab;
    private SerializedProperty _arrowSpawnPoint;
    private SerializedProperty _arrowSpawnPointRightLocalPosition;
    private SerializedProperty _healEffectPrefab;
    private SerializedProperty _deathSmokePrefab;
    private SerializedProperty _deathSmokeLifetime;
    private SerializedProperty _drawAttackRange;
    private SerializedProperty _drawVisionRange;
    private SerializedProperty _unitType;
    private SerializedProperty _ownerPlayerId;
    private SerializedProperty _teamColor;
    private SerializedProperty _moveSpeed;

    private void OnEnable()
    {
        _config = serializedObject.FindProperty("_config");
        _icon = serializedObject.FindProperty("_icon");
        _playerId = serializedObject.FindProperty("_playerId");
        _selectableByLocalPlayer = serializedObject.FindProperty("_selectableByLocalPlayer");
        _animator = serializedObject.FindProperty("_animator");
        _visualRoot = serializedObject.FindProperty("_visualRoot");
        _idleAnimationStateName = serializedObject.FindProperty("_idleAnimationStateName");
        _runAnimationStateName = serializedObject.FindProperty("_runAnimationStateName");
        _buildAnimationStateName = serializedObject.FindProperty("_buildAnimationStateName");
        _attackAnimationStateName = serializedObject.FindProperty("_attackAnimationStateName");
        _warriorAttackAnimationStateNameA = serializedObject.FindProperty("_warriorAttackAnimationStateNameA");
        _warriorAttackAnimationStateNameB = serializedObject.FindProperty("_warriorAttackAnimationStateNameB");
        _spearmanAttackRightAnimationStateName = serializedObject.FindProperty("_spearmanAttackRightAnimationStateName");
        _spearmanAttackUpAnimationStateName = serializedObject.FindProperty("_spearmanAttackUpAnimationStateName");
        _spearmanAttackDownAnimationStateName = serializedObject.FindProperty("_spearmanAttackDownAnimationStateName");
        _spearmanAttackUpRightAnimationStateName = serializedObject.FindProperty("_spearmanAttackUpRightAnimationStateName");
        _spearmanAttackDownRightAnimationStateName = serializedObject.FindProperty("_spearmanAttackDownRightAnimationStateName");
        _warriorAttackDuration = serializedObject.FindProperty("_warriorAttackDuration");
        _spearmanAttackDuration = serializedObject.FindProperty("_spearmanAttackDuration");
        _archerAttackDuration = serializedObject.FindProperty("_archerAttackDuration");
        _defaultAttackDuration = serializedObject.FindProperty("_defaultAttackDuration");
        _animationCrossFadeDuration = serializedObject.FindProperty("_animationCrossFadeDuration");
        _animationLayerIndex = serializedObject.FindProperty("_animationLayerIndex");
        _arrowPrefab = serializedObject.FindProperty("_arrowPrefab");
        _arrowSpawnPoint = serializedObject.FindProperty("_arrowSpawnPoint");
        _arrowSpawnPointRightLocalPosition = serializedObject.FindProperty("_arrowSpawnPointRightLocalPosition");
        _healEffectPrefab = serializedObject.FindProperty("_healEffectPrefab");
        _deathSmokePrefab = serializedObject.FindProperty("_deathSmokePrefab");
        _deathSmokeLifetime = serializedObject.FindProperty("_deathSmokeLifetime");
        _drawAttackRange = serializedObject.FindProperty("_drawAttackRange");
        _drawVisionRange = serializedObject.FindProperty("_drawVisionRange");
        _unitType = serializedObject.FindProperty("_unitType");
        _ownerPlayerId = serializedObject.FindProperty("_ownerPlayerId");
        _teamColor = serializedObject.FindProperty("_teamColor");
        _moveSpeed = serializedObject.FindProperty("_moveSpeed");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawCore();
        DrawRuntimeIdentity();
        DrawAnimation();
        DrawCombat();
        DrawDebug();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawCore()
    {
        EditorGUILayout.LabelField("Core", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_config);
        EditorGUILayout.PropertyField(_icon);
        EditorGUILayout.PropertyField(_unitType);
        EditorGUILayout.PropertyField(_teamColor);
        EditorGUILayout.PropertyField(_playerId);
        EditorGUILayout.PropertyField(_ownerPlayerId);
        EditorGUILayout.PropertyField(_selectableByLocalPlayer);
        EditorGUILayout.PropertyField(_moveSpeed);
        EditorGUILayout.Space();
    }

    private void DrawRuntimeIdentity()
    {
        EditorGUILayout.LabelField("References", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_animator);
        EditorGUILayout.PropertyField(_visualRoot);
        EditorGUILayout.Space();
    }

    private void DrawAnimation()
    {
        var unitType = (UnitType)_unitType.enumValueIndex;

        EditorGUILayout.LabelField("Animations", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_idleAnimationStateName);
        EditorGUILayout.PropertyField(_runAnimationStateName);

        if (unitType == UnitType.Worker)
            EditorGUILayout.PropertyField(_buildAnimationStateName);

        if (unitType == UnitType.Archer)
        {
            EditorGUILayout.PropertyField(_attackAnimationStateName);
            EditorGUILayout.PropertyField(_archerAttackDuration);
        }

        if (unitType == UnitType.Warrior)
        {
            EditorGUILayout.PropertyField(_warriorAttackAnimationStateNameA);
            EditorGUILayout.PropertyField(_warriorAttackAnimationStateNameB);
            EditorGUILayout.PropertyField(_warriorAttackDuration);
        }

        if (unitType == UnitType.Spearman)
        {
            EditorGUILayout.PropertyField(_spearmanAttackRightAnimationStateName);
            EditorGUILayout.PropertyField(_spearmanAttackUpAnimationStateName);
            EditorGUILayout.PropertyField(_spearmanAttackDownAnimationStateName);
            EditorGUILayout.PropertyField(_spearmanAttackUpRightAnimationStateName);
            EditorGUILayout.PropertyField(_spearmanAttackDownRightAnimationStateName);
            EditorGUILayout.PropertyField(_spearmanAttackDuration);
        }

        if (unitType != UnitType.Archer && unitType != UnitType.Warrior && unitType != UnitType.Spearman && unitType != UnitType.Worker && unitType != UnitType.Monk)
        {
            EditorGUILayout.PropertyField(_attackAnimationStateName);
            EditorGUILayout.PropertyField(_defaultAttackDuration);
        }

        EditorGUILayout.PropertyField(_animationCrossFadeDuration);
        EditorGUILayout.PropertyField(_animationLayerIndex);
        EditorGUILayout.Space();
    }

    private void DrawCombat()
    {
        var unitType = (UnitType)_unitType.enumValueIndex;

        EditorGUILayout.LabelField("Combat", EditorStyles.boldLabel);

        if (unitType == UnitType.Archer)
        {
            EditorGUILayout.PropertyField(_arrowPrefab);
            EditorGUILayout.PropertyField(_arrowSpawnPoint);
            EditorGUILayout.PropertyField(_arrowSpawnPointRightLocalPosition);
        }

        if (unitType == UnitType.Monk)
            EditorGUILayout.PropertyField(_healEffectPrefab);

        EditorGUILayout.PropertyField(_deathSmokePrefab);
        EditorGUILayout.PropertyField(_deathSmokeLifetime);
        EditorGUILayout.Space();
    }

    private void DrawDebug()
    {
        EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_drawAttackRange);
        EditorGUILayout.PropertyField(_drawVisionRange);
    }
}
#endif