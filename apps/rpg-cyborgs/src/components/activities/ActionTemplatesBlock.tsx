import {
  Button,
  Drawer,
  Grid,
  Heading,
  HStack,
  Stack,
  useDisclosure,
} from '@chakra-ui/react'
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
import { selectActionTemplates } from '@app/actions/actionTemplatesSelectors'

function ActionTemplatesBlock() {
  const playerCharacter = useSelector(selectPlayerCharacter)
  const actionTemplates = useSelector(selectActionTemplates)

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
      <Stack w={'100%'}>
        <Heading as="h3" size="lg" paddingBottom={4} paddingTop={10}>
          Actions
        </Heading>
        <HStack w={'100%'} alignItems={'stretch'}>
          <StatPanel
            name={'Action Points'}
            abbreviatedName={''}
            prop={'CurrentActionPoints'}
            propValue={actionPoints}
          />
          <StatPanel
            name={'Focus Points'}
            abbreviatedName={''}
            prop={'CurrentFocusPoints'}
            propValue={focusPoints}
          />
          <StatPanel
            name={'Luck Points'}
            abbreviatedName={''}
            prop={'CurrentLuckPoints'}
            propValue={luckPoints}
          />
          <StatPanel
            name={'Reactions'}
            abbreviatedName={''}
            prop={'Value'}
            propValue={reactions}
          />
        </HStack>
        <Grid templateColumns="repeat(6, 1fr)" gap={6}>
          {actionTemplates.map((actionTemplate, i) => (
            <ActionTemplatePanel
              key={i}
              actionTemplate={actionTemplate}
              onActionTemplate={(actionTemplate) =>
                onActionTemplateButtonClicked(actionTemplate)
              }
            />
          ))}
        </Grid>
      </Stack>
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
