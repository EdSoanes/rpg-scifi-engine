import React, { useState } from 'react'
import { ModSetDescription, State } from '../../lib/rpg-api/types'
import {
  Button,
  Code,
  IconButton,
  DrawerRoot,
  DrawerBody,
  DrawerCloseTrigger,
  DrawerContent,
  DrawerFooter,
  DrawerHeader,
  DrawerOverlay,
  Stack,
  useColorMode,
  useDisclosure,
} from '@chakra-ui/react'
import {
  CheckCircleIcon,
  QuestionOutlineIcon,
  SmallCloseIcon,
} from '@chakra-ui/icons'
import {
  selectGraphState,
  selectPlayerCharacter,
} from '../../app/graphState/graphSelectors'
import { useSelector } from 'react-redux'
import { useAppDispatch } from '../../app/hooks'
import { toggleState } from '../../app/thunks'
import { getStateDescription } from '../../lib/rpg-api/fetcher'

export declare interface TimePanelProps {
  state: State
}

function TimePanel(props: TimePanelProps) {
  const dispatch = useAppDispatch()
  const graphState = useSelector(selectGraphState)
  const playerCharacter = useSelector(selectPlayerCharacter)
  const variant = props.state.isOn ? 'solid' : 'outline'
  const [describe, setDescribe] = useState<ModSetDescription | undefined>()

  const { colorMode } = useColorMode()
  const { onOpen, onClose, isOpen } = useDisclosure()

  const onChangeState = async () => {
    if (playerCharacter) {
      dispatch(
        toggleState({
          entityId: playerCharacter.id,
          state: props.state.name,
          on: !props.state.isOn,
        })
      )
    }
  }

  const onDescribe = async () => {
    if (props?.state) {
      const response = await getStateDescription(
        props.state.ownerId,
        props.state.name,
        graphState!
      )
      setDescribe(response?.data)
      onOpen()
    }
  }

  return (
    <>
      <Stack direction={'row'} gap={0}>
        <Button
          leftIcon={props.state.isOn ? <CheckCircleIcon /> : <SmallCloseIcon />}
          variant={variant}
          size={'lg'}
          onClick={onChangeState}
        >
          {props.state.name}
        </Button>
        <IconButton
          marginLeft={0}
          paddingLeft={0}
          variant={'unstyled'}
          aria-label="describe"
          size="lg"
          icon={<QuestionOutlineIcon />}
          onClick={onDescribe}
        />
      </Stack>
      {colorMode === 'light' && (
        <DrawerRoot isOpen={isOpen} onClose={onClose}>
          <DrawerContent>
            <DrawerHeader>{describe?.name ?? '-'}</DrawerHeader>
            <DrawerCloseTrigger />
            <DrawerBody>
              <Code>{JSON.stringify(describe, null, 2)}</Code>
            </DrawerBody>

            <DrawerFooter>
              <Button
                variant={'unstyled'}
                colorScheme="blue"
                mr={3}
                onClick={onClose}
              >
                Close
              </Button>
            </DrawerFooter>
          </DrawerContent>
        </DrawerRoot>
      )}
    </>
  )
}

export default TimePanel
