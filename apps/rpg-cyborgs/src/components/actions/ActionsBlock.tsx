import { atom, useAtom } from 'jotai'
import {
  Button,
  Drawer,
  DrawerBody,
  DrawerCloseButton,
  DrawerContent,
  DrawerFooter,
  DrawerHeader,
  DrawerOverlay,
  Grid,
  Heading,
  Input,
  Stack,
  useDisclosure,
} from '@chakra-ui/react'
import React from 'react'
import { splitAtom } from 'jotai/utils'
import { playerCharacterActionsAtom } from '../atoms/playerCharacterActions.atom'
import ActionButton from './ActionButton'
import { Action } from '../../lib/rpg-api/types'

const actionAtomsAtom = splitAtom(playerCharacterActionsAtom)
const selectedActionAtom = atom<Action | null>(null)

function ActionsBlock() {
  const { isOpen, onOpen, onClose } = useDisclosure()
  const [selectedAction, setSelectedAction] = useAtom(selectedActionAtom)

  const onActionButtonClicked = (action: Action) => {
    setSelectedAction(action)
    onOpen()
  }

  const [actionAtoms] = useAtom(actionAtomsAtom)
  return (
    <>
      <Stack w={'100%'}>
        <Heading as="h3" size="lg" paddingBottom={4} paddingTop={10}>
          Action
        </Heading>
        <Grid templateColumns="repeat(6, 1fr)" gap={6}>
          {actionAtoms.map((state, i) => (
            <ActionButton
              key={i}
              actionAtom={state}
              onAction={onActionButtonClicked}
            />
          ))}
        </Grid>
      </Stack>
      <Drawer isOpen={isOpen} size={'lg'} placement="right" onClose={onClose}>
        <DrawerOverlay />
        <DrawerContent>
          <DrawerCloseButton />
          <DrawerHeader>{selectedAction?.name ?? '-'}</DrawerHeader>

          <DrawerBody>
            <Input placeholder="Type here..." />
          </DrawerBody>

          <DrawerFooter>
            <Button variant="outline" mr={3} onClick={onClose}>
              Cancel
            </Button>
            <Button colorScheme="blue">Save</Button>
          </DrawerFooter>
        </DrawerContent>
      </Drawer>
    </>
  )
}

export default ActionsBlock
