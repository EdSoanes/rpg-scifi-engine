import { createSelector } from '@reduxjs/toolkit'
import {
  ActionTemplate,
  PointInTime,
  RpgGraphState,
  State,
} from '../../lib/rpg-api/types'

import { RootState } from '../store'
import { PlayerCharacter, PropValue } from '../../lib/rpg-api/cyborg-types'

export const selectGraphState = (state: RootState): RpgGraphState | undefined =>
  state.graph.graphState

export const selectActionTemplates = (state: RootState): ActionTemplate[] => 
  state.actionTemplates.actionTemplates

export const selectStateName = (state: RootState, stateName: string) =>
  stateName

export const selectActionName = (state: RootState, actionName: string) =>
  actionName

export const selectTime = createSelector(
  [selectGraphState],
  (graphState?: RpgGraphState): PointInTime | undefined => {
    return graphState?.time?.now
  }
)

export const selectPlayerCharacter = createSelector(
  [selectGraphState],
  (graphState?: RpgGraphState): PlayerCharacter | undefined => {
    return graphState?.entities.find(
      (entity) => entity.archetype === 'PlayerCharacter'
    ) as PlayerCharacter
  }
)

export const selectPlayerCharacterStates = createSelector(
  [selectPlayerCharacter],
  (playerCharacter?: PlayerCharacter): State[] => {
    const dict = playerCharacter?.states ?? {}
    return Object.entries(dict).map((pair) => pair[1] as State)
  }
)

export const selectStrength = createSelector(
  [selectPlayerCharacter],
  (playerCharacter?: PlayerCharacter): PropValue | undefined => {
    return playerCharacter?.strength
  }
)

export const selectAgility = createSelector(
  [selectPlayerCharacter],
  (playerCharacter?: PlayerCharacter): PropValue | undefined => {
    return playerCharacter?.agility
  }
)

export const selectHealth = createSelector(
  [selectPlayerCharacter],
  (playerCharacter?: PlayerCharacter): PropValue | undefined => {
    return playerCharacter?.health
  }
)

export const selectBrains = createSelector(
  [selectPlayerCharacter],
  (playerCharacter?: PlayerCharacter): PropValue | undefined => {
    return playerCharacter?.brains
  }
)

export const selectInsight = createSelector(
  [selectPlayerCharacter],
  (playerCharacter?: PlayerCharacter): PropValue | undefined => {
    return playerCharacter?.insight
  }
)

export const selectCharisma = createSelector(
  [selectPlayerCharacter],
  (playerCharacter?: PlayerCharacter): PropValue | undefined => {
    return playerCharacter?.charisma
  }
)

export const selectMeleeAttack = createSelector(
  [selectPlayerCharacter],
  (playerCharacter?: PlayerCharacter): PropValue | undefined => {
    return playerCharacter?.meleeAttack
  }
)

export const selectRangedAttack = createSelector(
  [selectPlayerCharacter],
  (playerCharacter?: PlayerCharacter): PropValue | undefined => {
    return playerCharacter?.rangedAttack
  }
)

export const selectDefence = createSelector(
  [selectPlayerCharacter],
  (playerCharacter?: PlayerCharacter): PropValue | undefined => {
    return playerCharacter?.defence
  }
)

export const selectReactions = createSelector(
  [selectPlayerCharacter],
  (playerCharacter?: PlayerCharacter): PropValue | undefined => {
    return playerCharacter?.reactions
  }
)

export const selectActionPoints = createSelector(
  [selectPlayerCharacter],
  (playerCharacter?: PlayerCharacter): PropValue | undefined => {
    return playerCharacter
      ? ({
          id: `${playerCharacter.id}/ap`,
          value: playerCharacter.currentActionPoints,
          baseValue: playerCharacter.actionPoints,
        } as PropValue)
      : undefined
  }
)

export const selectFocusPoints = createSelector(
  [selectPlayerCharacter],
  (playerCharacter?: PlayerCharacter): PropValue | undefined => {
    return playerCharacter
      ? ({
          id: `${playerCharacter.id}/fp`,
          value: playerCharacter.currentFocusPoints,
          baseValue: playerCharacter.focusPoints,
        } as PropValue)
      : undefined
  }
)

export const selectLuckPoints = createSelector(
  [selectPlayerCharacter],
  (playerCharacter?: PlayerCharacter): PropValue | undefined => {
    return playerCharacter
      ? ({
          id: `${playerCharacter.id}/lp`,
          value: playerCharacter.currentLuckPoints,
          baseValue: playerCharacter.luckPoints,
        } as PropValue)
      : undefined
  }
)

export const selectStaminaPoints = createSelector(
  [selectPlayerCharacter],
  (playerCharacter?: PlayerCharacter): PropValue | undefined => {
    return playerCharacter
      ? ({
          id: `${playerCharacter.id}/stp`,
          value: playerCharacter.currentStaminaPoints,
          baseValue: playerCharacter.staminaPoints,
        } as PropValue)
      : undefined
  }
)

export const selectLifePoints = createSelector(
  [selectPlayerCharacter],
  (playerCharacter?: PlayerCharacter): PropValue | undefined => {
    return playerCharacter
      ? ({
          id: `${playerCharacter.id}/lfp`,
          value: playerCharacter.currentLifePoints,
          baseValue: playerCharacter.lifePoints,
        } as PropValue)
      : undefined
  }
)

export const selectActionByName = createSelector(
  [selectActionTemplates, selectActionName],
  (actions, actionName) => actions.find((action) => action.name === actionName)
)

export const selectHead = createSelector(
  [selectPlayerCharacter],
  (playerCharacter?: PlayerCharacter) => playerCharacter?.head
)

export const selectTorso = createSelector(
  [selectPlayerCharacter],
  (playerCharacter?: PlayerCharacter) => playerCharacter?.torso
)

export const selectLeftArm = createSelector(
  [selectPlayerCharacter],
  (playerCharacter?: PlayerCharacter) => playerCharacter?.leftArm
)

export const selectRightArm = createSelector(
  [selectPlayerCharacter],
  (playerCharacter?: PlayerCharacter) => playerCharacter?.rightArm
)

export const selectLeftLeg = createSelector(
  [selectPlayerCharacter],
  (playerCharacter?: PlayerCharacter) => playerCharacter?.leftLeg
)

export const selectRightLeg = createSelector(
  [selectPlayerCharacter],
  (playerCharacter?: PlayerCharacter) => playerCharacter?.rightLeg
)
