import { Button, Drawer, Grid, GridItem, useDisclosure } from '@chakra-ui/react'
import { useState } from 'react'
import ActionTemplatePanel from './ActionTemplatePanel'

import { useSelector } from 'react-redux'
import {
  selectActionPoints,
  selectFocusPoints,
  selectLuckPoints,
  selectPlayerCharacter,
  selectReactions,
} from '@app/graphState/graphSelectors'
import { initiateAction } from '@app/thunks'
import { useAppDispatch } from '@app/hooks'

import { ActionTemplate } from '@lib/rpg-api/types'
import { StatPanel } from '../stats'
import ActionInstancePanel from './ActivityPanel'
import {
  selectActionTemplates,
  selectSkillTemplates,
} from '@app/actions/actionTemplatesSelectors'
import { TimeBlock } from '@components/time'
import SkillTemplatePanel from './SkillTemplatePanel'

function ActionTemplatesBlock() {
  const playerCharacter = useSelector(selectPlayerCharacter)
  const actionTemplates = useSelector(selectActionTemplates)
  const skillTemplates = useSelector(selectSkillTemplates)

  const actionPoints = useSelector(selectActionPoints)
  const focusPoints = useSelector(selectFocusPoints)
  const luckPoints = useSelector(selectLuckPoints)
  const reactions = useSelector(selectReactions)

  const dispatch = useAppDispatch()
  const { onOpen, onClose } = useDisclosure()
  const [selectedActionTemplate, setSelectedActionTemplate] = useState<
    ActionTemplate | undefined
  >()

  const onActionTemplateButtonClicked = async (
    actionTemplate: ActionTemplate
  ) => {
    setSelectedActionTemplate(actionTemplate)
    console.log('onActionButtonClicked', actionTemplate)
    if (playerCharacter) {
      await dispatch(
        initiateAction({
          actionTemplateOwnerId: actionTemplate.ownerId,
          initiatorId: playerCharacter.id,
          actionTemplateName: actionTemplate.name,
        })
      )

      onOpen()
    }
  }

  return (
    <>
      <Grid w={'100%'} templateColumns={'repeat(6, 1fr)'}>
        <GridItem colSpan={6}>
          <TimeBlock />
        </GridItem>
        <GridItem colSpan={1}>
          <Grid templateColumns={'repeat(1, 1fr)'}>
            <GridItem>
              <StatPanel
                name={'Action Points'}
                abbreviatedName={''}
                prop={'CurrentActionPoints'}
                propValue={actionPoints}
              />
            </GridItem>
            <GridItem>
              <StatPanel
                name={'Focus Points'}
                abbreviatedName={''}
                prop={'CurrentFocusPoints'}
                propValue={focusPoints}
              />
            </GridItem>
            <GridItem>
              <StatPanel
                name={'Luck Points'}
                abbreviatedName={''}
                prop={'CurrentLuckPoints'}
                propValue={luckPoints}
              />
            </GridItem>
            <GridItem>
              <StatPanel
                name={'Reactions'}
                abbreviatedName={''}
                prop={'Value'}
                propValue={reactions}
              />
            </GridItem>
          </Grid>
        </GridItem>
        <GridItem colSpan={1}></GridItem>

        <GridItem colSpan={2}>
          <Grid templateColumns="repeat(1, 1fr)" gap={6}>
            {actionTemplates.map((actionTemplate, i) => (
              <GridItem key={i}>
                <ActionTemplatePanel
                  actionTemplate={actionTemplate}
                  onActionTemplate={(actionTemplate) =>
                    onActionTemplateButtonClicked(actionTemplate)
                  }
                />
              </GridItem>
            ))}
          </Grid>
        </GridItem>
        <GridItem colSpan={2}>
          <Grid templateColumns="repeat(1, 1fr)" gap={6}>
            {skillTemplates.map((skillTemplate, i) => (
              <GridItem key={i}>
                <SkillTemplatePanel
                  skillTemplate={skillTemplate}
                  onSkillTemplate={(actionTemplate) =>
                    onActionTemplateButtonClicked(actionTemplate)
                  }
                />
              </GridItem>
            ))}
          </Grid>
        </GridItem>
      </Grid>
      <Drawer.Root>
        <Drawer.Content>
          <Drawer.Header>{selectedActionTemplate?.name ?? '-'}</Drawer.Header>

          <Drawer.Body>
            <ActionInstancePanel />
          </Drawer.Body>

          <Drawer.Footer>
            <Button variant="outline" mr={3} onClick={onClose}>
              Cancel
            </Button>
            <Button colorScheme="blue">Save</Button>
          </Drawer.Footer>
        </Drawer.Content>
      </Drawer.Root>
    </>
  )
}

export default ActionTemplatesBlock
