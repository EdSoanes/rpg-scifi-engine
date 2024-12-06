import { createSelector } from '@reduxjs/toolkit'
import { ActionTemplate } from '../../lib/rpg-api/types'
import { RootState } from '../store'
import { ThunkStatus } from '../thunks'
import { SkillTemplate } from '@/lib/rpg-api/cyborg-types'

export const selectActionTemplates = (state: RootState): ActionTemplate[] =>
  state.actionTemplates.actionTemplates.filter(
    (item) => item.classification == 'Action'
  )
export const selectSkillTemplates = (state: RootState): SkillTemplate[] =>
  state.actionTemplates.actionTemplates.filter(
    (item) => item.classification == 'Skill'
  ) as SkillTemplate[]
export const selectActionName = (state: RootState, actionName: string) =>
  actionName
export const selectActionsStatus = (state: RootState): ThunkStatus =>
  state.actionTemplates.status

export const selectActionTemplateByName = createSelector(
  [selectActionTemplates, selectActionName],
  (actionTemplates, actionTemplateName) =>
    actionTemplates.find(
      (actionTemplate) => actionTemplate.name === actionTemplateName
    )
)
