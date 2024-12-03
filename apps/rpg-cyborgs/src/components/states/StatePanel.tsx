import { useState } from 'react'
import { ModSetDescription, State } from '../../lib/rpg-api/types'
import {
  Button,
  Code,
  IconButton,
  Drawer,
  Stack,
  useDisclosure,
  Heading,
} from '@chakra-ui/react'
import {
  selectGraphState,
  selectPlayerCharacter,
} from '../../app/graphState/graphSelectors'
import { useSelector } from 'react-redux'
import { useAppDispatch } from '../../app/hooks'
import { toggleState } from '../../app/thunks'
import { getStateDescription } from '../../lib/rpg-api/fetcher'
import BoxButton from '../ui/box-button'

export declare interface StateButtonProps {
  state: State
}

function StatePanel(props: StateButtonProps) {
  const dispatch = useAppDispatch()
  const graphState = useSelector(selectGraphState)
  const playerCharacter = useSelector(selectPlayerCharacter)
  const [describe, setDescribe] = useState<ModSetDescription | undefined>()

  const { onOpen, onClose } = useDisclosure()

  const onChangeState = async () => {
    console.log('clicked', playerCharacter)
    if (playerCharacter) {
      console.log('playercharacter', playerCharacter)
      await dispatch(
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
        <BoxButton onClick={onChangeState}>
          <Heading as={'h3'} size={'sm'}>
            {props.state.name}
          </Heading>{' '}
          <IconButton
            marginLeft={0}
            paddingLeft={0}
            aria-label="describe"
            size="lg"
            onClick={onDescribe}
          />
        </BoxButton>
      </Stack>
      <Drawer.Root>
        <Drawer.Content>
          <Drawer.Header>{describe?.name ?? '-'}</Drawer.Header>
          <Drawer.Body>
            <Code>{JSON.stringify(describe, null, 2)}</Code>
          </Drawer.Body>

          <Drawer.Footer>
            <Button colorScheme="blue" mr={3} onClick={onClose}>
              Close
            </Button>
          </Drawer.Footer>
        </Drawer.Content>
      </Drawer.Root>
    </>
  )
}

export default StatePanel
