import { createSelector } from "@reduxjs/toolkit"
import { ActionTemplate } from "../../lib/rpg-api/types"
import { RootState } from "../store"
import { ThunkStatus } from "../thunks"

export const selectActionTemplates = (state: RootState): ActionTemplate[] => state.actionTemplates.actionTemplates
export const selectActionName = (state: RootState, actionName: string) => actionName
export const selectActionsStatus = (state: RootState): ThunkStatus => state.actionTemplates.status

export const selectActionTemplateByName = createSelector(
  [selectActionTemplates, selectActionName],
  (actionTemplates, actionTemplateName) => actionTemplates.find((actionTemplate) => actionTemplate.name === actionTemplateName)
)
