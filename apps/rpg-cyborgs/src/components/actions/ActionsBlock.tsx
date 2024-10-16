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
  Stack,
  StatGroup,
  useDisclosure,
} from '@chakra-ui/react'
import React from 'react'
import { splitAtom } from 'jotai/utils'
import { playerCharacterActionsAtom } from '../atoms/playerCharacterActions.atom'
import ActionButton from './ActionButton'
import {
  Action,
  ActionInstance,
  Activity,
  PropValue,
} from '../../lib/rpg-api/types'
import { playerCharacterAtom } from '../atoms/playerCharacter.atom'
import { StatPanel } from '../stats'
import { getActionInstance } from '../../lib/rpg-api/fetcher'
import { graphStateAtom } from '../atoms/graphState.atom'
import ActionInstancePanel from './ActionInstancePanel'

const actionAtomsAtom = splitAtom(playerCharacterActionsAtom)
const selectedActionAtom = atom<Action | undefined>(undefined)
const activityAtom = atom<Activity | undefined>(undefined)
const actionInstanceAtom = atom<ActionInstance | undefined>(undefined)

const reactionsAtom = atom<PropValue | null>(
  (get) => get(playerCharacterAtom)?.reactions ?? null
)

const actionPointsAtom = atom<PropValue | null>((get) => {
  const pc = get(playerCharacterAtom)
  return {
    id: pc?.id,
    value: pc?.currentActionPoints,
    baseValue: pc?.actionPoints,
  } as PropValue
})

const focusPointsAtom = atom<PropValue | null>((get) => {
  const pc = get(playerCharacterAtom)
  return {
    id: pc?.id,
    value: pc?.currentFocusPoints,
    baseValue: pc?.focusPoints,
  } as PropValue
})

const luckPointsAtom = atom<PropValue | null>((get) => {
  const pc = get(playerCharacterAtom)
  return {
    id: pc?.id,
    value: pc?.currentLuckPoints,
    baseValue: pc?.luckPoints,
  } as PropValue
})

function ActionsBlock() {
  const { isOpen, onOpen, onClose } = useDisclosure()
  const [selectedAction, setSelectedAction] = useAtom(selectedActionAtom)
  const [activity, setActivity] = useAtom(activityAtom)
  const [actionInstance, setActionInstance] = useAtom(actionInstanceAtom)

  const [playerCharacter] = useAtom(playerCharacterAtom)
  const [graphState, setGraphState] = useAtom(graphStateAtom)

  const onActionButtonClicked = async (action: Action) => {
    if (playerCharacter != null) {
      const res = await getActionInstance(
        playerCharacter?.id,
        playerCharacter?.id,
        action.name,
        graphState!
      )

      setSelectedAction(action)
      setActivity(res?.data)
      setActionInstance(res?.data?.actionInstance)

      onOpen()
    }
  }

  const [actionAtoms] = useAtom(actionAtomsAtom)
  return (
    <>
      <Stack w={'100%'}>
        <Heading as="h3" size="lg" paddingBottom={4} paddingTop={10}>
          Action
        </Heading>
        <StatGroup w={'100%'} alignItems={'stretch'}>
          <StatPanel
            propName={'Action Points'}
            propNameAbbr={''}
            propValueAtom={actionPointsAtom}
          />
          <StatPanel
            propName={'Focus Points'}
            propNameAbbr={''}
            propValueAtom={focusPointsAtom}
          />
          <StatPanel
            propName={'Luck Points'}
            propNameAbbr={''}
            propValueAtom={luckPointsAtom}
          />
          <StatPanel
            propName={'Reactions'}
            propNameAbbr={''}
            propValueAtom={reactionsAtom}
          />
        </StatGroup>
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
            <ActionInstancePanel
              activityAtom={activityAtom}
              actionInstanceAtom={actionInstanceAtom}
            />
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
