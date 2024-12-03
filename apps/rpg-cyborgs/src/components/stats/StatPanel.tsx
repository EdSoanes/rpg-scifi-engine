import {
  Stat,
  IconButton,
  useDisclosure,
  Code,
  Button,
  Drawer,
} from '@chakra-ui/react'
import { useState } from 'react'
import { PropDescription } from '../../lib/rpg-api/types'
import { getPropDesc } from '../../lib/rpg-api/fetcher'
import { selectGraphState } from '../../app/graphState/graphSelectors'
import { useSelector } from 'react-redux'
import { PropValue } from '../../lib/rpg-api/cyborg-types'

//const describeAtom = atom<PropDesc | undefined>(undefined)

export declare interface StatPanelProps {
  propName: string
  propNameAbbr: string
  propValue?: PropValue
}

function StatPanel(props: StatPanelProps) {
  const graphState = useSelector(selectGraphState)
  const [describe, setDescribe] = useState<PropDescription | undefined>()

  const eq =
    (props?.propValue?.value ?? 0) === (props?.propValue?.baseValue ?? 0)
  const inc =
    !eq && (props?.propValue?.value ?? 0) > (props?.propValue?.baseValue ?? 0)
  const dec =
    !eq && (props?.propValue?.value ?? 0) < (props?.propValue?.baseValue ?? 0)

  const onDescribe = async () => {
    if (props?.propValue) {
      const response = await getPropDesc(
        props.propValue.id,
        'Value',
        graphState!
      )
      setDescribe(response?.data)
      onOpen()
    }
  }

  const { onOpen, onClose } = useDisclosure()

  return (
    <>
      <Stat.Root
        m={4}
        p={4}
        border="1px"
        borderRadius={4}
        borderColor={'lightgray'}
      >
        <Stat.Label>{props.propNameAbbr}</Stat.Label>
        <Stat.ValueText>{props?.propValue?.value ?? 0}</Stat.ValueText>
        <Stat.HelpText>
          {inc && <Stat.UpIndicator />}
          {dec && <Stat.DownIndicator />}
          {props.propName} {props?.propValue?.baseValue ?? 0}
          <IconButton
            variant={'ghost'}
            aria-label="describe"
            size="lg"
            onClick={onDescribe}
          />
        </Stat.HelpText>
      </Stat.Root>
      <Drawer.Root>
        <Drawer.Content>
          <Drawer.Header>{describe?.rootProp ?? '-'}</Drawer.Header>
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

export default StatPanel
