import React from 'react'
import {
  //Code,
  Stack,
  // Tab,
  // TabList,
  // TabPanel,
  // TabPanels,
  // Tabs,
} from '@chakra-ui/react'
import ActionPanel from './ActionPanel'
import { selectActions } from '../../app/activity/activitySelectors'
import { useSelector } from 'react-redux'
import { selectActionsStatus } from '../../app/actions/actionsSelectors'

function ActivityPanel() {

  const actions = useSelector(selectActions)
  const actionStatus = useSelector(selectActionsStatus)

  return (
    actionStatus === 'loaded' &&
    <Stack w={'100%'}>
      {actions.map((i) => (
        <ActionPanel key={(`${i.name}/${i.actionNo}`)} action={i} />
      ))}
    </Stack>
  )
}

export default ActivityPanel
