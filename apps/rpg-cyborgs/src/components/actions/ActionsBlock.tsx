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
import React, { useState } from 'react'
import ActionButton from './ActionButton'
import { Action } from '../../lib/rpg-api/types'
import { StatPanel } from '../stats'
import ActionInstancePanel from './ActivityPanel'
import { selectActions } from '../../app/actions/actionsSelectors'
import { useSelector } from 'react-redux'
import {
  selectActionPoints,
  selectFocusPoints,
  selectLuckPoints,
  selectPlayerCharacter,
  selectReactions,
} from '../../app/graphState/graphSelectors'
// import { selectActionInstance, selectActivity, selectActivityStatus } from '../../app/activity/activitySelectors'
import { fetchActivity } from '../../app/thunks'
import { useAppDispatch } from '../../app/hooks'

// const reactionsAtom = atom<PropValue | null>(
//   (get) => get(playerCharacterAtom)?.reactions ?? null
// )

// const actionPointsAtom = atom<PropValue | null>((get) => {
//   const pc = get(playerCharacterAtom)
//   return {
//     id: pc?.id,
//     value: pc?.currentActionPoints,
//     baseValue: pc?.actionPoints,
//   } as PropValue
// })

// const focusPointsAtom = atom<PropValue | null>((get) => {
//   const pc = get(playerCharacterAtom)
//   return {
//     id: pc?.id,
//     value: pc?.currentFocusPoints,
//     baseValue: pc?.focusPoints,
//   } as PropValue
// })

// const luckPointsAtom = atom<PropValue | null>((get) => {
//   const pc = get(playerCharacterAtom)
//   return {
//     id: pc?.id,
//     value: pc?.currentLuckPoints,
//     baseValue: pc?.luckPoints,
//   } as PropValue
// })

function ActionsBlock() {
  const playerCharacter = useSelector(selectPlayerCharacter)
  const actions = useSelector(selectActions)

  const actionPoints = useSelector(selectActionPoints)
  const focusPoints = useSelector(selectFocusPoints)
  const luckPoints = useSelector(selectLuckPoints)
  const reactions = useSelector(selectReactions)

  // const activity = useSelector(selectActivity)
  // const activityStatus = useSelector(selectActivityStatus)
  // const actionInstance = useSelector(selectActionInstance)

  const dispatch = useAppDispatch()
  const { isOpen, onOpen, onClose } = useDisclosure()
  const [selectedAction, setSelectedAction] = useState<Action | undefined>()

  const onActionButtonClicked = async (action: Action) => {
    setSelectedAction(action)

    if (playerCharacter) {
      dispatch(
        fetchActivity({
          ownerId: action.ownerId,
          initiatorId: playerCharacter.id,
          action: action.name,
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
        <StatGroup w={'100%'} alignItems={'stretch'}>
          <StatPanel
            propName={'Action Points'}
            propNameAbbr={''}
            propValue={actionPoints}
          />
          <StatPanel
            propName={'Focus Points'}
            propNameAbbr={''}
            propValue={focusPoints}
          />
          <StatPanel
            propName={'Luck Points'}
            propNameAbbr={''}
            propValue={luckPoints}
          />
          <StatPanel
            propName={'Reactions'}
            propNameAbbr={''}
            propValue={reactions}
          />
        </StatGroup>
        <Grid templateColumns="repeat(6, 1fr)" gap={6}>
          {actions.map((action, i) => (
            <ActionButton
              key={i}
              action={action}
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
            <ActionInstancePanel />
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
