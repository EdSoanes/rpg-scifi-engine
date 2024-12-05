import {
  Button,
  Drawer,
  Grid,
  Heading,
  Stack,
  useDisclosure,
} from '@chakra-ui/react'
import { useState } from 'react'
import ActionTemplatePanel from './ActionTemplatePanel'

import { useSelector } from 'react-redux'
import { selectPlayerCharacter } from '../../app/graphState/graphSelectors'
import { initiateAction } from '../../app/thunks'
import { useAppDispatch } from '../../app/hooks'

import { ActionTemplate } from '../../lib/rpg-api/types'
import ActionInstancePanel from './ActivityPanel'
import { selectSkillTemplates } from '../../app/actions/actionTemplatesSelectors'

function SkillTemplatesBlock() {
  const playerCharacter = useSelector(selectPlayerCharacter)
  const skillTemplates = useSelector(selectSkillTemplates)

  const dispatch = useAppDispatch()
  const { onOpen, onClose } = useDisclosure()
  const [selectedSkillTemplate, setSelectedSkillTemplate] = useState<
    ActionTemplate | undefined
  >()

  const onSkillTemplateButtonClicked = async (
    actionTemplate: ActionTemplate
  ) => {
    setSelectedSkillTemplate(actionTemplate)
    console.log('onSkillButtonClicked', actionTemplate)
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
        <Grid templateColumns="repeat(6, 1fr)" gap={6}>
          {skillTemplates.map((skillTemplate, i) => (
            <ActionTemplatePanel
              key={i}
              actionTemplate={skillTemplate}
              onActionTemplate={(skillTemplate) =>
                onSkillTemplateButtonClicked(skillTemplate)
              }
            />
          ))}
        </Grid>
      </Stack>
      <Drawer.Root>
        <Drawer.Content>
          <Drawer.Header>{selectedSkillTemplate?.name ?? '-'}</Drawer.Header>

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

export default SkillTemplatesBlock
